using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;
using workwear.Models.Import;
using workwear.Tools.Nhibernate;
using workwear.ViewModels.Import;
using Workwear.Measurements;
using workwear.Repository.Company;
using workwear.Repository.Operations;
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
		public void ItemsLoad_VostokCase()
		{
			NewSessionWithSameDB();
			using(var uowPrepare = UnitOfWorkFactory.CreateWithoutRoot()) {
				MakeMeasurementUnits(uowPrepare, out MeasurementUnits sht, out MeasurementUnits pair);
				MakeSizes(uowPrepare);

				var subdivision = new Subdivision {
					Name = "500305 Мехслужба"
				};
				uowPrepare.Save(subdivision);
				
				var post = new Post {
					Name = "Слесарь-ремонтник",
					Subdivision = subdivision
				};
				uowPrepare.Save(post);

				var glovesType = new ItemsType() {
					Name = "Перчатки",
					Category = ItemTypeCategory.wear,
					Units = pair
				};
				uowPrepare.Save(glovesType);
				
				var bootsType = new ItemsType() {
					Name = "Обувь",
					Category = ItemTypeCategory.wear,
					Units = pair
				};
				uowPrepare.Save(bootsType);
				
				var PPEType = new ItemsType() {
					Name = "Сизод",
					Category = ItemTypeCategory.wear,
					Units = sht
				};
				uowPrepare.Save(PPEType);
				
				var suitType = new ItemsType() {
					Name = "Костюмы",
					Category = ItemTypeCategory.wear,
					Units = sht
				};
				uowPrepare.Save(suitType);

				var protection1 = new ProtectionTools {
					Name = "Ботинки кожаные с защитным подноском",
					Type = bootsType,
				};
				uowPrepare.Save(protection1);
				
				var protection2 = new ProtectionTools {
					Name = "Перчатки с полимерным  покрытием",
					Type = glovesType,
				};
				uowPrepare.Save(protection2);
				
				var protection3 = new ProtectionTools {
					Name = "Перчатки «Хайкрон»",
					Type = glovesType,
				};
				uowPrepare.Save(protection3);
				
				var protection4 = new ProtectionTools {
					Name = "СИЗОД фильтрующее (1 класс защиты)",
					Type = PPEType,
				};
				uowPrepare.Save(protection4);
				
				var protection5 = new ProtectionTools {
					Name = "Костюм для защиты от общих производственных загрязнений и механических воздействий",
					Type = suitType,
				};
				uowPrepare.Save(protection5);

				var norm = new Norm();
				norm.AddPost(post);
				norm.AddItem(protection1);
				norm.AddItem(protection2);
				norm.AddItem(protection3);
				norm.AddItem(protection4);
				norm.AddItem(protection5);
				uowPrepare.Save(norm);

				var employee = new EmployeeCard() {
					PersonnelNumber = "4865",
					LastName = "Абысов",
					FirstName = "Алексей",
					Patronymic = "Юрьевич",
					Subdivision = subdivision,
					Post = post
				};
				employee.AddUsedNorm(norm);
				uowPrepare.Save(employee);
				uowPrepare.Commit();
				
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
					Assert.That(itemsLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
					itemsLoad.ThirdStep();
					Assert.That(itemsLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
					itemsLoad.Save();

					var uow = itemsLoad.UoW;
					var savedEmployee = uow.GetById<EmployeeCard>(employee.Id);
					Assert.That(savedEmployee.FirstName, Is.EqualTo("Алексей"));
					var wearitemSuit = savedEmployee.WorkwearItems.First(x => x.ProtectionTools.Id == protection5.Id);
					Assert.That(wearitemSuit.Amount, Is.EqualTo(1));
					Assert.That(wearitemSuit.LastIssue, Is.EqualTo(new DateTime(2020, 6, 18)));
					var wearitemGloves = savedEmployee.WorkwearItems.First(x => x.ProtectionTools.Id == protection2.Id);
					Assert.That(wearitemGloves.Amount, Is.EqualTo(12));
					Assert.That(wearitemGloves.LastIssue, Is.EqualTo(new DateTime(2021, 5, 18)));
					var wearitemPPE = savedEmployee.WorkwearItems.First(x => x.ProtectionTools.Id == protection4.Id);
					Assert.That(wearitemPPE.Amount, Is.EqualTo(5));
					Assert.That(wearitemPPE.LastIssue, Is.EqualTo(new DateTime(2021, 5, 18)));
					
					//Проверяем создание операций...
					var issueRepository = new EmployeeIssueRepository(uow);
					var operationsForBoots = issueRepository.GetOperationsForEmployee(employee, protection1);
					Assert.That(operationsForBoots, Has.Count.EqualTo(2));
					var operation1 = operationsForBoots.First(x => x.Nomenclature.Name == "Ботинки кожаные мужские р. 44");
					Assert.That(operation1.StartOfUse, Is.EqualTo(new DateTime(2019, 4, 4)));
					Assert.That(operation1.Issued, Is.EqualTo(1));
					Assert.That(operation1.WearSize.Name, Is.EqualTo("44"));
					var operation2 = operationsForBoots.First(x => x.Nomenclature.Name == "Ботинки ТОФФ ТРУД МП чер. МП");
					Assert.That(operation2.StartOfUse, Is.EqualTo(new DateTime(2020, 6, 18)));
					Assert.That(operation2.Issued, Is.EqualTo(1));
					Assert.That(operation2.WearSize.Name, Is.EqualTo("43"));
					var operationsForSuit = issueRepository.GetOperationsForEmployee(employee, protection5);
					Assert.That(operationsForSuit, Has.Count.EqualTo(1));
					var operation3 = operationsForSuit.First();
					Assert.That(operation3.Nomenclature.Name, Is.EqualTo("Костюм БАЙКАЛ-1 т-син смесовая"));
					Assert.That(operation3.StartOfUse, Is.EqualTo(new DateTime(2020, 6, 18)));
					Assert.That(operation3.Issued, Is.EqualTo(1));
					Assert.That(operation3.WearSize.Name, Is.EqualTo("52-54")); //Тут мы предполагаем что алгоритм сможет конвертировать размер из формата по госту в формат общепринятого размера.
					Assert.That(operation3.Height.Name, Is.EqualTo("170-176"));
					
					//Проверяем заполнение размеров в сотруднике по последним выдачам
					var employeeHeight = savedEmployee.Sizes.First(x => x.SizeType.Id == 1);
					Assert.That(employeeHeight.Size.Name, Is.EqualTo("170-176"));
					var employeeWearSize = savedEmployee.Sizes.First(x => x.SizeType.Id == 2);
					Assert.That(employeeWearSize.Size.Name, Is.EqualTo("52-54"));
					var employeeShoesSize = savedEmployee.Sizes.First(x => x.SizeType.Id == 4);
					Assert.That(employeeShoesSize.Size.Name, Is.EqualTo("44").Or.EqualTo("43"));
					var employeeGlovesSize = savedEmployee.Sizes.First(x => x.SizeType.Id == 7);
					Assert.That(employeeGlovesSize.Size.Name, Is.EqualTo("9").Or.EqualTo("10"));
				}
			}
		}

		#region Helpers

		private void MakeMeasurementUnits(IUnitOfWork uow, out MeasurementUnits sht, out MeasurementUnits pair) {
			sht = new MeasurementUnits {
				Name = "шт.",
				OKEI = "796"
			};
			uow.Save(sht);
			pair = new MeasurementUnits {
				Name = "пара",
				OKEI = "715"
			};
			uow.Save(pair);
		}
		
		private void MakeSizes(IUnitOfWork uow) {
			var heightType = new SizeType
				{Id  = 1, Name = "Рост", Position = 1, CategorySizeType = CategorySizeType.Height, UseInEmployee = true};
			uow.Save(heightType, orUpdate: false);
			
			uow.Save(new Size {Name = "170-176", SizeType = heightType, UseInEmployee = true, UseInNomenclature = true});
			
			var sizeType = new SizeType 
				{Id = 2, Name = "Размер одежды", Position = 2, CategorySizeType = CategorySizeType.Size, UseInEmployee = true};
			uow.Save(sizeType, orUpdate: false);
			
			uow.Save(new Size {Name = "52-54", SizeType = sizeType, AlternativeName = "104-108", UseInEmployee = true, UseInNomenclature = true});

			var shoesType = new SizeType 
				{Id = 4, Name = "Обувь", Position = 3, CategorySizeType = CategorySizeType.Size, UseInEmployee = true};
			uow.Save(shoesType, orUpdate: false);
			
			uow.Save(new Size {Name = "43", SizeType = shoesType, UseInEmployee = true, UseInNomenclature = true});
			uow.Save(new Size {Name = "44", SizeType = shoesType, UseInEmployee = true, UseInNomenclature = true});
			
			var glovesType = new SizeType 
				{Id = 7, Name = "Перчатки", Position = 8, CategorySizeType = CategorySizeType.Size, UseInEmployee = true};
			uow.Save(glovesType, orUpdate: false);
			
			uow.Save(new Size {Name = "9", SizeType = glovesType, UseInEmployee = true, UseInNomenclature = true});
			uow.Save(new Size {Name = "10", SizeType = glovesType, UseInEmployee = true, UseInNomenclature = true});
			
			uow.Commit();
		}

		#endregion
	}
}