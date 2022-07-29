using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Testing.DB;
using workwear.Domain.Regulations;
using Workwear.Domain.Regulations;
using workwear.Models.Import;
using workwear.Tools.Nhibernate;
using workwear.ViewModels.Import;
using Workwear.Measurements;
using workwear.Repository.Company;
using workwear.Repository.Regulations;
using workwear.Repository.Stock;

namespace WorkwearTest.Integration.Import
{
	[TestFixture(TestOf = typeof(ExcelImportViewModel))]
	[Category("Integrated")]
	public class ExcelImportViewModelItemsTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[TearDown]
		public void TestTearDown()
		{
			NewSessionWithNewDB();
		}
		
		[Test(Description = "Проверяем загрузку норм выданного в формате выгруженном из Восток-Сервис.")]
		public void NormsLoad_VostokCase()
		{
			NewSessionWithSameDB();
			using(var uowSaveSize = UnitOfWorkFactory.CreateWithoutRoot()) {
				MakeMeasurementUnits(uowSaveSize);
				
				var navigation = Substitute.For<INavigationManager>();
				var interactive = Substitute.For<IInteractiveMessage>();
				var progressStep = Substitute.For<IProgressBarDisplayable>();
				var progressInterceptor = Substitute.For<ProgressInterceptor>();
				var setting = new SettingsWorkwearItemsViewModel();
				var dataparser = new DataParserWorkwearItems(new NomenclatureRepository(), new PostRepository(), new NormRepository(), new SizeService());
				var model = new ImportModelWorkwearItems(dataparser, setting);
				using(var itemsLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
					itemsLoad.ProgressStep = progressStep;
					itemsLoad.FileName = "Samples/Excel/items_vostok.xls";
					Assert.That(itemsLoad.Sheets.Count, Is.EqualTo(1));
					itemsLoad.SelectedSheet = itemsLoad.Sheets.First();
					Assert.That(itemsLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
					itemsLoad.SecondStep();
					model.Columns[2].DataTypeByLevels[0].DataType = model.DataTypes.First(x => DataTypeNorm.ProtectionTools.Equals(x.Data)) ; //Третья колонка номенклатура нормы.
					Assert.That(itemsLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
					itemsLoad.ThirdStep();
					Assert.That(itemsLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
					itemsLoad.Save();

					var uow = itemsLoad.UoW;
					var norms = uow.GetAll<Norm>().ToList();
					Assert.That(norms.Count, Is.EqualTo(7));
					var coordinator = norms.First(x => x.Posts.Any(p => p.Name == "Координатор производства"));
					Assert.That(coordinator.Items.Count, Is.GreaterThanOrEqualTo(6));
					var jaket = coordinator.Items.First(x =>
						x.ProtectionTools.Name ==
						"Куртка для защиты от общих производственных загрязнений и механических воздействий на утепляющей прокладке");
					Assert.That(jaket.PeriodCount, Is.EqualTo(2));
					Assert.That(jaket.NormPeriod, Is.EqualTo(NormPeriodType.Year));
					var vest = coordinator.Items.First(x => x.ProtectionTools.Name == "жилет утеплённый");
					Assert.That(vest.NormPeriod, Is.EqualTo(NormPeriodType.Wearout));

					var operatorsNorm = norms.First(x => x.Posts.Any(p => p.Name == "Оператор 1 разряда"));
					Assert.That(operatorsNorm.Posts, Has.Exactly(5).Items);
					Assert.That(operatorsNorm.Posts.Any(x => x.Name == "Оператор 4 разряда"), Is.True);
					var gloves = operatorsNorm.Items.First(x => x.ProtectionTools.Name.ToLower() == "перчатки с полимерным покрытием");
					Assert.That(gloves.Amount, Is.EqualTo(12));
				}
			}
		}

		#region Helpers

		private void MakeMeasurementUnits(IUnitOfWork uow)
		{
			uow.Save(new MeasurementUnits {
				Name = "шт.",
				OKEI = "796"
			});
			uow.Save(new MeasurementUnits {
				Name = "пара",
				OKEI = "715"
			});
			uow.Commit();
		}

		#endregion
	}
}
