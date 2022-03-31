using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Navigation;
using QS.Testing.DB;
using workwear.Domain.Company;
using Workwear.Domain.Company;
using workwear.Models.Company;
using workwear.Models.Import;
using workwear.Repository.Company;
using workwear.Tools.Nhibernate;
using workwear.ViewModels.Import;
using Workwear.Measurements;

namespace WorkwearTest.Integration.Import
{
	[TestFixture(TestOf = typeof(ExcelImportViewModel))]
	[Category("Integrated")]
	public class ExcelImportViewModelTest : InMemoryDBGlobalConfigTestFixtureBase
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
		
		[Test(Description = "Проверяем что без проблем можем загрузить файл формата со Стойленского ГОК")]
		public void EmployeesLoad_StolenskyGok()
		{
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var sizeSettings = Substitute.For<ISizeSettings>();
			sizeSettings.EmployeeSizeRanges.Returns(false);
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(sizeSettings));
			var setting = new SettingsMatchEmployeesViewModel();
			var model = new ImportModelEmployee(dataparser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/cardkey_list.xlsx";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();

				var uow = employeesLoad.UoW;
				var employees = uow.GetAll<EmployeeCard>().ToList();
				Assert.That(employees.Count, Is.EqualTo(9));
				var sergey = employees.First(x => x.PersonnelNumber == "58391");
				Assert.That(sergey.CardKey, Is.EqualTo("80313E3A538A04"));
				Assert.That(sergey.LastName, Is.EqualTo("Волчихин"));
				Assert.That(sergey.FirstName, Is.EqualTo("Сергей"));
				Assert.That(sergey.Patronymic, Is.EqualTo("Владимирович"));
				Assert.That(sergey.Sex, Is.EqualTo(Sex.M));
			}
		}
		
		[Test(Description = "Проверяем что без проблем можем загрузить файл формата со ОСМиБТ")]
		public void EmployeesLoad_Vostok1c()
		{
			//В файле дата хранится в виде строки, поэтому для прохождения теста, нужна русская культура
			Thread.CurrentThread.CurrentCulture =  CultureInfo.CreateSpecificCulture("ru-RU");
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var sizeSettings = Substitute.For<ISizeSettings>();
			sizeSettings.EmployeeSizeRanges.Returns(false);
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(sizeSettings));
			var setting = new SettingsMatchEmployeesViewModel();
			//Так же проверяем что табельные номера вида 00002 превратятся в "2"
			setting.ConvertPersonnelNumber = true;
			var model = new ImportModelEmployee(dataparser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/vostok_1c_employee.xls";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();

				var uow = employeesLoad.UoW;
				var employees = uow.GetAll<EmployeeCard>().ToList();
				Assert.That(employees.Count, Is.EqualTo(4));
				var olga = employees.First(x => x.PersonnelNumber == "1");
				Assert.That(olga.LastName, Is.EqualTo("Гриднева"));
				Assert.That(olga.FirstName, Is.EqualTo("Ольга"));
				Assert.That(olga.Patronymic, Is.EqualTo("Николаевна"));
				Assert.That(olga.HireDate, Is.EqualTo(new DateTime(2020, 3, 10)));
				Assert.That(olga.Sex, Is.EqualTo(Sex.F));
				Assert.That(olga.Subdivision.Name, Is.EqualTo("500006 Отдел главного энергетика"));
				Assert.That(olga.Post.Name, Is.EqualTo("Ведущий инженер"));
				Assert.That(olga.Post.Subdivision.Name, Is.EqualTo("500006 Отдел главного энергетика"));
				
				//Проверяем что должности из разных подразделений не сливаются.
				var natalia = employees.First(x => x.PersonnelNumber == "2");
				Assert.That(natalia.Subdivision.Name, Is.EqualTo("500007 Отдел главного механика"));
				Assert.That(natalia.Post.Name, Is.EqualTo("Ведущий инженер"));
				Assert.That(natalia.Post.Subdivision.Name, Is.EqualTo("500007 Отдел главного механика"));
				
				//Проверяем что не дублируем должности и подразделения.
				var igor = employees.First(x => x.PersonnelNumber == "3");
				var ury = employees.First(x => x.PersonnelNumber == "5");
				Assert.That(igor.Subdivision.Name, Is.EqualTo("500300 Цех санитарных керамических изделий"));
				Assert.That(igor.Post.Name, Is.EqualTo("Изготовитель капов (из эпоксидной смолы)."));
				Assert.That(igor.Post.Subdivision.Name, Is.EqualTo("500300 Цех санитарных керамических изделий"));
				Assert.That(igor.Subdivision.Id, Is.EqualTo(ury.Subdivision.Id));
				Assert.That(igor.Post.Id, Is.EqualTo(ury.Post.Id));
				Assert.That(igor.Post.Subdivision.Id, Is.EqualTo(ury.Post.Subdivision.Id));
			}
		}
		
		[Test(Description = "Проверяем что без проблем можем загрузить файл формата со ОСМиБТ и можем обновить сотрудника")]
		public void EmployeesLoad_osmbtDepartments()
		{
			NewSessionWithSameDB();
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var sizeSettings = Substitute.For<ISizeSettings>();
			sizeSettings.EmployeeSizeRanges.Returns(false);
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(sizeSettings));
			var setting = new SettingsMatchEmployeesViewModel();
			//Так же проверяем что табельные номера вида 00002 превратятся в "2"
			setting.ConvertPersonnelNumber = true;
			var model = new ImportModelEmployee(dataparser, setting);
			
			using (var rootUow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var existEmployee = new EmployeeCard
				{
					PersonnelNumber = "5213",
					LastName = "Старая фамилия",
					FirstName = "Старое имя",
					Comment = "Старый комментарий"
				};
				rootUow.Save(existEmployee);
				rootUow.Commit();
				
				using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
					var uow = employeesLoad.UoW;
					var employeesold = uow.GetAll<EmployeeCard>().ToList();
					Assert.That(employeesold.Count, Is.EqualTo(1));
					
					employeesLoad.ProgressStep = progressStep;
					employeesLoad.FileName = "Samples/Excel/osmbt.xls";
					Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
					employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
					Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
					employeesLoad.SecondStep();
					Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
					employeesLoad.ThirdStep();
					Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
					employeesLoad.Save();

					var employees = uow.GetAll<EmployeeCard>().ToList();
					Assert.That(employees.Count, Is.EqualTo(2));
					var artem = employees.First(x => x.PersonnelNumber == "5213");
					Assert.That(artem.FirstName, Is.EqualTo("Артем"));
					Assert.That(artem.LastName, Is.EqualTo("Беляев"));
					Assert.That(artem.Comment, Is.EqualTo("Старый комментарий")); //Фамилия и имя заменились, комментарий остался старым.
					Assert.That(artem.Subdivision.Name, Is.EqualTo("500100 Цех керамического кирпича"));
					Assert.That(artem.Department.Name, Is.EqualTo("Участок Е1 Бригада 7  (Е1)"));
					
					//Проверяем что не дублируем должности и подразделения.
					var igor = employees.First(x => x.PersonnelNumber == "4103");
					Assert.That(igor.Subdivision.Name, Is.EqualTo("500100 Цех керамического кирпича"));
					Assert.That(igor.Post.Name, Is.EqualTo("Заведующий хозяйством"));
					Assert.That(igor.Department.Name, Is.EqualTo("Участок 500100"));
				}
			}
		}

		[Test(Description = "Проверяем что нормально работаем с датами при чтении даты поступления и увольнения")]
		public void EmployeesLoad_DateWorks()
		{
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var sizeSettings = Substitute.For<ISizeSettings>();
			sizeSettings.EmployeeSizeRanges.Returns(false);
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(sizeSettings));
			var setting = new SettingsMatchEmployeesViewModel();
			var model = new ImportModelEmployee(dataparser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				var importModel = employeesLoad.ImportModel as ImportModelEmployee;
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/dismissed_employees.xls";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				importModel.Columns[1].DataType = DataTypeEmployee.DismissDate; //Вторая колонка дата увольнения.
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();

				var uow = employeesLoad.UoW;
				var employees = uow.GetAll<EmployeeCard>().ToList();
				
				Assert.That(employees.Count, Is.EqualTo(2));
				var anastasia = employees.First(x => x.FirstName == "Анастасия");
				Assert.That(anastasia.LastName, Is.EqualTo("Устинова"));
				Assert.That(anastasia.Patronymic, Is.EqualTo("Владимировна"));
				Assert.That(anastasia.HireDate, Is.EqualTo(new DateTime(2006, 4, 4)));
				Assert.That(anastasia.Sex, Is.EqualTo(Sex.F));
				Assert.That(anastasia.DismissDate, Is.EqualTo(new DateTime(2021, 3, 31)));
				
				var natalia = employees.First(x => x.FirstName == "Наталья");
				Assert.That(natalia.HireDate, Is.EqualTo(new DateTime(2020, 12, 11)));
				Assert.That(natalia.Sex, Is.EqualTo(Sex.F));
				Assert.That(natalia.DismissDate, Is.EqualTo(new DateTime(2021, 1, 13)));
			}
		}
		
		[Test(Description = "Проверяем что нормально работаем с файлами имеющими пустые строки + размеры сотрудника")]
		public void EmployeesLoad_EmptyRows_Sizes_A2Case()
		{
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var sizeSettings = Substitute.For<ISizeSettings>();
			sizeSettings.EmployeeSizeRanges.Returns(true);
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(sizeSettings));
			var setting = new SettingsMatchEmployeesViewModel();
			var model = new ImportModelEmployee(dataparser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				var importModel = employeesLoad.ImportModel as ImportModelEmployee;
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/empty_first_row_a2.xls";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();

				var uow = employeesLoad.UoW;
				var employees = uow.GetAll<EmployeeCard>().ToList();
				
				Assert.That(employees.Count, Is.EqualTo(5));
				var nikolay = employees.First(x => x.FirstName == "Николай");
				Assert.That(nikolay.WearGrowth, Is.EqualTo("170-176"));
				Assert.That(nikolay.WearSize, Is.EqualTo("48-50"));
				Assert.That(nikolay.ShoesSize, Is.EqualTo("38"));
				
				//Проверяем что должности не задублировались
				var posts = uow.GetAll<Post>();
				Assert.That(posts.Count, Is.EqualTo(3));
			}
		}
	}
}
