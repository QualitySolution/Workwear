using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Testing.DB;
using Workwear.Domain.Regulations;
using Workwear.Models.Regulations;

using EtnNormItem = QS.Cloud.WorkwearDictionary.NormItem;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.ItemSIZ;
using EtnGetNormResponse = QS.Cloud.WorkwearDictionary.GetNormResponse;
using EtnComplectType = QS.Cloud.WorkwearDictionary.ComplectType;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.PeriodType;

namespace Workwear.Test.Integration.Regulations {

	[TestFixture(TestOf = typeof(EtnNormImportModel))]
	[Category("Integrated")]
	public class EtnNormImportModelTest : InMemoryDBGlobalConfigTestFixtureBase {

		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Приложения 2/3 добавляют строки в норму независимо от приложения 1 - не должны трогать её название/комментарий/должности.")]
		public void AddItemsFromEtn_DoesNotChangeNormNameCommentOrPosts() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var norm = new Norm { Name = "Исходное название", Comment = "Исходный комментарий" };

				var etnResponse = new EtnGetNormResponse {
					NumberNorm = "1.1.1",
					NormName = "Контакт с кислотами",
					Items = {
						new EtnNormItem {
							ComplectType = EtnComplectType.One,
							Items = { new EtnItemSIZ { SizName = "Перчатки кислотостойкие", SizType = "Перчатки", Amount = 1, PeriodCount = 1, PeriodType = EtnPeriodType.Year } }
						}
					}
				};

				var model = new EtnNormImportModel(uow, norm, Substitute.For<IInteractiveService>(), Substitute.For<IEtnComplectResolver>());
				model.AddItemsFromEtn(etnResponse, appendixNumber: 2);

				Assert.That(norm.Name, Is.EqualTo("Исходное название"));
				Assert.That(norm.Comment, Is.EqualTo("Исходный комментарий"));
				Assert.That(norm.Posts, Is.Empty);
				Assert.That(norm.Items, Has.Count.EqualTo(1));
				Assert.That(norm.Items.First().ProtectionTools.Name, Is.EqualTo("Перчатки кислотостойкие"));
				Assert.That(norm.Items.First().NormParagraph, Does.Contain("Приложение 2"));
			}
		}

		[Test(Description = "Блоки, соединённые общим alt_group (приложение 2, 'или' на уровне блоков) - в норму должен попасть только блок, выбранный резолвером.")]
		public void AddItemsFromEtn_AltGroupWithMultipleBlocks_AddsOnlyChosenBlock() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var norm = new Norm();

				var blockA = new EtnNormItem {
					ComplectType = EtnComplectType.One,
					ComplectName = "Гидрофобные средства",
					AltGroup = 5,
					Items = { new EtnItemSIZ { SizName = "Крем гидрофобный", Amount = 1, PeriodCount = 1, PeriodType = EtnPeriodType.Month } }
				};
				var blockB = new EtnNormItem {
					ComplectType = EtnComplectType.One,
					ComplectName = "Гидрофильные средства",
					AltGroup = 5,
					Items = { new EtnItemSIZ { SizName = "Крем гидрофильный", Amount = 1, PeriodCount = 1, PeriodType = EtnPeriodType.Month } }
				};

				var resolver = Substitute.For<IEtnComplectResolver>();
				resolver.ResolveAltGroup(Arg.Any<System.Collections.Generic.IList<EtnNormItem>>()).Returns(blockB);

				var etnResponse = new EtnGetNormResponse { NumberNorm = "2.1.1", NormName = "Загрязнение", Items = { blockA, blockB } };

				var model = new EtnNormImportModel(uow, norm, Substitute.For<IInteractiveService>(), resolver);
				model.AddItemsFromEtn(etnResponse, appendixNumber: 3);

				Assert.That(norm.Items, Has.Count.EqualTo(1));
				Assert.That(norm.Items.First().ProtectionTools.Name, Is.EqualTo("Крем гидрофильный"));
			}
		}

		[Test(Description = "Если в alt_group только один блок (остальные альтернативы уже отфильтрованы сервером) - резолвер не спрашивается, блок добавляется как обычно.")]
		public void AddItemsFromEtn_AltGroupWithSingleBlock_DoesNotAskResolver() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var norm = new Norm();
				var etnResponse = new EtnGetNormResponse {
					NumberNorm = "2.1.1",
					NormName = "Загрязнение",
					Items = {
						new EtnNormItem {
							ComplectType = EtnComplectType.One,
							AltGroup = 9,
							Items = { new EtnItemSIZ { SizName = "Крем защитный", Amount = 1, PeriodCount = 1, PeriodType = EtnPeriodType.Month } }
						}
					}
				};

				var resolver = Substitute.For<IEtnComplectResolver>();
				var model = new EtnNormImportModel(uow, norm, Substitute.For<IInteractiveService>(), resolver);
				model.AddItemsFromEtn(etnResponse, appendixNumber: 2);

				resolver.DidNotReceive().ResolveAltGroup(Arg.Any<System.Collections.Generic.IList<EtnNormItem>>());
				Assert.That(norm.Items, Has.Count.EqualTo(1));
			}
		}

		[Test(Description = "ItemSIZ.dermal_ppe (приложение 3) должно переноситься на создаваемую ProtectionTools.DermalPpe.")]
		public void AddItemsFromEtn_DermalItem_SetsDermalPpeOnCreatedProtectionTools() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var norm = new Norm();
				var etnResponse = new EtnGetNormResponse {
					NumberNorm = "3.1",
					NormName = "Смывающее средство",
					Items = {
						new EtnNormItem {
							ComplectType = EtnComplectType.One,
							Items = { new EtnItemSIZ { SizName = "Мыло жидкое", Amount = 200, PeriodCount = 1, PeriodType = EtnPeriodType.Month, DermalPpe = true } }
						}
					}
				};

				var model = new EtnNormImportModel(uow, norm, Substitute.For<IInteractiveService>(), Substitute.For<IEtnComplectResolver>());
				model.AddItemsFromEtn(etnResponse, appendixNumber: 3);

				Assert.That(norm.Items.First().ProtectionTools.DermalPpe, Is.True);
			}
		}

		[Test(Description = "is_additional (доп. СИЗ по результатам оценки профрисков) должен отмечаться в комментарии строки нормы.")]
		public void AddItemsFromEtn_IsAdditionalComplect_AddsMarkerToRowComment() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var norm = new Norm();
				var etnResponse = new EtnGetNormResponse {
					NumberNorm = "2.1.1",
					NormName = "Загрязнение",
					Items = {
						new EtnNormItem {
							ComplectType = EtnComplectType.And,
							ComplectName = "Дополнительный комплект",
							IsAdditional = true,
							Items = { new EtnItemSIZ { SizName = "Очки защитные", Amount = 1, PeriodCount = 1, PeriodType = EtnPeriodType.Year } }
						}
					}
				};

				var model = new EtnNormImportModel(uow, norm, Substitute.For<IInteractiveService>(), Substitute.For<IEtnComplectResolver>());
				model.AddItemsFromEtn(etnResponse, appendixNumber: 2);

				Assert.That(norm.Items.First().Comment, Does.Contain("Доп. СИЗ по результатам оценки профрисков"));
				Assert.That(norm.Items.First().Comment, Does.Contain("Дополнительный комплект"));
			}
		}

		[Test(Description = "PERIOD_TYPE_DUTY (новое в приложении 2) должен маппиться на дежурный период выдачи, а не падать в дефолт 'Год'.")]
		public void MapPeriodType_Duty_MapsToNormPeriodTypeDuty() {
			Assert.That(EtnNormImportModel.MapPeriodType(EtnPeriodType.Duty), Is.EqualTo(NormPeriodType.Duty));
		}

		[Test(Description = "Норма может заполняться из нескольких приложений ЕТН подряд одним и тем же экземпляром модели - " +
			"авто-созданная при втором импорте номенклатура должна откатываться при удалении строки так же, как и при первом.")]
		public void AddItemsFromEtn_CalledTwiceOnSameModel_RollsBackAutoCreatedFromSecondCall() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var norm = new Norm();
				var model = new EtnNormImportModel(uow, norm, Substitute.For<IInteractiveService>(), Substitute.For<IEtnComplectResolver>());

				model.AddItemsFromEtn(new EtnGetNormResponse {
					NumberNorm = "1", NormName = "Первый импорт",
					Items = { new EtnNormItem { ComplectType = EtnComplectType.One,
						Items = { new EtnItemSIZ { SizName = "Перчатки", Amount = 1, PeriodCount = 1, PeriodType = EtnPeriodType.Year } } } }
				}, appendixNumber: 2);

				model.AddItemsFromEtn(new EtnGetNormResponse {
					NumberNorm = "2", NormName = "Второй импорт",
					Items = { new EtnNormItem { ComplectType = EtnComplectType.One,
						Items = { new EtnItemSIZ { SizName = "Очки", Amount = 1, PeriodCount = 1, PeriodType = EtnPeriodType.Year } } } }
				}, appendixNumber: 3);

				Assert.That(norm.Items, Has.Count.EqualTo(2));

				var secondItem = norm.Items.Single(i => i.ProtectionTools.Name == "Очки");
				norm.RemoveItem(secondItem);

				Assert.That(uow.Session.QueryOver<ProtectionTools>().Where(x => x.Name == "Очки").List(), Is.Empty);
				Assert.That(norm.Items, Has.Count.EqualTo(1));
				Assert.That(norm.Items.First().ProtectionTools.Name, Is.EqualTo("Перчатки"));
			}
		}
	}
}
