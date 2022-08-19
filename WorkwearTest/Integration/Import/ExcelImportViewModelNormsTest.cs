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
using workwear.Models.Import.Norms;
using workwear.Repository.Regulations;

namespace WorkwearTest.Integration.Import
{
	[TestFixture(TestOf = typeof(ExcelImportViewModel))]
	[Category("Integrated")]
	public class ExcelImportViewModelNormsTest : InMemoryDBGlobalConfigTestFixtureBase
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
		
		[Test(Description = "Проверяем загрузку норм из формата Агроном-а, особенностью является наличие объединенных ячеек и должностей через запятую.")]
		public void NormsLoad_AgronomCase()
		{
			NewSessionWithSameDB();
			using(var uowSaveSize = UnitOfWorkFactory.CreateWithoutRoot()) {
				MakeMeasurementUnits(uowSaveSize);
				
				var navigation = Substitute.For<INavigationManager>();
				var interactive = Substitute.For<IInteractiveMessage>();
				var progressStep = Substitute.For<IProgressBarDisplayable>();
				var progressInterceptor = Substitute.For<ProgressInterceptor>();
				var dataparser = new DataParserNorm(new NormRepository(), new ProtectionToolsRepository(), new SizeService());
				var model = new ImportModelNorm(dataparser);
				using(var normsLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
					normsLoad.ProgressStep = progressStep;
					normsLoad.FileName = "Samples/Excel/norms_agronom.xlsx";
					Assert.That(normsLoad.Sheets.Count, Is.EqualTo(3));
					normsLoad.SelectedSheet = normsLoad.Sheets.First();
					Assert.That(normsLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
					normsLoad.SecondStep();
					model.Columns[2].DataTypeByLevels[0].DataType = model.DataTypes.First(x => DataTypeNorm.ProtectionTools.Equals(x.Data)) ; //Третья колонка номенклатура нормы.
					Assert.That(normsLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
					normsLoad.ThirdStep();
					Assert.That(normsLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
					normsLoad.Save();

					var uow = normsLoad.UoW;
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
