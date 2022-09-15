using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dapper;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Navigation;
using QS.Testing.DB;
using QS.Utilities.Numeric;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using workwear.Models.Company;
using workwear.Tools.Nhibernate;
using workwear.ViewModels.Import;
using Workwear.Measurements;
using workwear.Models.Import.Employees;

namespace WorkwearTest.Integration.Import
{
	[TestFixture(TestOf = typeof(ExcelImportViewModel))]
	[Category("Integrated")]
	public class ExcelImportViewModelEmployeeTest : InMemoryDBGlobalConfigTestFixtureBase
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
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
			var model = new ImportModelEmployee(dataparser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/Employees/cardkey_list.xlsx";
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
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
			//Так же проверяем что табельные номера вида 00002 превратятся в "2"
			setting.ConvertPersonnelNumber = true;
			var model = new ImportModelEmployee(dataparser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/Employees/vostok_1c_employee.xls";
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
				Assert.That(olga.Post.Subdivision?.Name, Is.EqualTo("500006 Отдел главного энергетика"));
				
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
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
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
					employeesLoad.FileName = "Samples/Excel/Employees/osmbt.xls";
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
			var dataparser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
			var model = new ImportModelEmployee(dataparser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				var importModel = employeesLoad.ImportModel as ImportModelEmployee;
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/Employees/dismissed_employees.xls";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				importModel.Columns[1].DataTypeByLevels[0].DataType = importModel.DataTypes.First(x => DataTypeEmployee.DismissDate.Equals(x.Data)) ; //Вторая колонка дата увольнения.
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
			NewSessionWithSameDB();
				
			var uowSaveSize = UnitOfWorkFactory.CreateWithoutRoot();
			var heightType = new SizeType
				{Name = "Рост", Position = 1, UseInEmployee = true, CategorySizeType = CategorySizeType.Height};
			uowSaveSize.Save(heightType);
			var sizeType = new SizeType 
				{Name = "Размер", Position = 2, CategorySizeType = CategorySizeType.Size, UseInEmployee = true};
			uowSaveSize.Save(sizeType);
			var shoesType = new SizeType 
				{Name = "Обувь", Position = 3, CategorySizeType = CategorySizeType.Size, UseInEmployee = true};
			uowSaveSize.Save(shoesType);
			var height = new Size {Name = "170-176", SizeType = heightType, ShowInEmployee = true};
			uowSaveSize.Save(height);
			var size = new Size {Name = "48-50", SizeType = sizeType, ShowInEmployee = true};
			uowSaveSize.Save(size);
			var shoes = new Size {Name = "38", SizeType = shoesType, ShowInEmployee = true};
			uowSaveSize.Save(shoes);
			uowSaveSize.Commit();

			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var dataParser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
			var model = new ImportModelEmployee(dataParser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor))
			{
				var uow = employeesLoad.UoW;
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/Employees/empty_first_row_a2.xls";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();
				uow.Commit();
				
				var employees = uow.GetAll<EmployeeCard>().ToList();

				Assert.That(employees.Count, Is.EqualTo(5));
				var nikolay = employees.First(x => x.FirstName == "Николай");
				Assert.That(nikolay.Sizes.FirstOrDefault(x => x.SizeType.Id == heightType.Id)?.Size?.Id, Is.EqualTo(height.Id));
				Assert.That(nikolay.Sizes.FirstOrDefault(x => x.SizeType.Id == sizeType.Id)?.Size?.Id, Is.EqualTo(size.Id));
				Assert.That(nikolay.Sizes.FirstOrDefault(x => x.SizeType.Id == shoesType.Id)?.Size?.Id, Is.EqualTo(shoes.Id));
				
				//Проверяем что должности не задублировались, внимание "Менеджер по персоналу" разные должности для разных подразделений.
				var posts = uow.GetAll<Post>();
				Assert.That(posts.Count, Is.EqualTo(3));
			}
		}
		
		[Test(Description = "Проверяем возможность загрузить большинство стандартных размеров. Так же загрузку телефона, установку пола по " +
		                    "имени и автоматическое преобразование роста к стандарту ГОСТ")]
		public void EmployeesLoad_StandardSizes_AgronomCase()
		{
			NewSessionWithSameDB();
			var uowSaveSize = UnitOfWorkFactory.CreateWithoutRoot();
			InsertStandardSizes(uowSaveSize.Session.Connection);

			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var dataParser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
			var model = new ImportModelEmployee(dataParser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor))
			{
				var uow = employeesLoad.UoW;
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/Employees/all_sizes.xlsx";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();
				uow.Commit();
				
				var employees = uow.GetAll<EmployeeCard>().ToList();

				Assert.That(employees.Count, Is.EqualTo(3));
				var employee1 = employees.First(x => x.FirstName == "Эйваз");
				Assert.That(employee1.PhoneNumber, Is.EqualTo("+7-980-353-02-10"));
				Assert.That(employee1.Sex, Is.EqualTo(Sex.M));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("182"));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 2)?.Size?.Name, Is.EqualTo("48-50"));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 6)?.Size?.Name, Is.EqualTo("58"));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 10)?.Size?.Name, Is.EqualTo("3"));
				
				var employee2 = employees.First(x => x.FirstName == "Виктор");
				Assert.That(employee2.Sex, Is.EqualTo(Sex.M));
				//Ниже должен сработать механизм конвертации в рост по госту
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("170"));
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 11)?.Size?.Name, Is.EqualTo("2"));
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 8)?.Size?.Name, Is.EqualTo("2"));
				
				var employee3 = employees.First(x => x.FirstName == "Ирина");
				Assert.That(employee3.Sex, Is.EqualTo(Sex.F));
				Assert.That(employee3.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("170-176"));
				Assert.That(employee3.Sizes.FirstOrDefault(x => x.SizeType.Id == 4)?.Size?.Name, Is.EqualTo("39"));
				Assert.That(employee3.Sizes.FirstOrDefault(x => x.SizeType.Id == 7)?.Size?.Name, Is.EqualTo("9"));
			}
		}
		
		[Test(Description = "Проверяем что можем сопоставить буквы Ё и Е при сопоставлении с имеющимися сотрудниками. " +
		                    "Бонусом проверяем работу обхвата груди.")]
		public void EmployeesLoad_YoInNameCase()
		{
			NewSessionWithSameDB();
			var uowSaveSize = UnitOfWorkFactory.CreateWithoutRoot();
			InsertStandardSizes(uowSaveSize.Session.Connection);
			var roman = new EmployeeCard() {
				LastName = "Боровлев", // Важно для теста! В файле через букву "ё"
				FirstName = "Роман",
				Patronymic = "Алексеевич"
			};
			uowSaveSize.Save(roman);
			
			var alex = new EmployeeCard() {
				LastName = "Пономарёв", // Важно для теста! В файле "е" вместо "ё"
				FirstName = "Алексей",
				Patronymic = "Сергеевич"
			};
			uowSaveSize.Save(alex);
			uowSaveSize.Commit();

			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var dataParser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
			var model = new ImportModelEmployee(dataParser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor))
			{
				var uow = employeesLoad.UoW;
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/Employees/employees_yo_in_name.xlsx";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();
				uow.Commit();
				
				var employees = uow.GetAll<EmployeeCard>().ToList();

				Assert.That(employees.Count, Is.EqualTo(2));
				var employee1 = employees.First(x => x.FirstName == "Роман");
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("164"));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 2)?.Size?.Name, Is.EqualTo("50"));

				var employee2 = employees.First(x => x.FirstName == "Алексей");
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("182"));
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 2)?.Size?.Name, Is.EqualTo("68"));
				
				//Проверяем что не обновляем значения полей если отличие в букве ё. То есть лучше оставить вариант который в базе чем вариант из файла.
				Assert.That(employee1.LastName, Is.EqualTo("Боровлев"));
				Assert.That(employee2.LastName, Is.EqualTo("Пономарёв"));
			}
		}
		
		[Test(Description = "Проверяем что можем сопоставить сотрудников если в файле только Фамилия и инициалы. ")]
		public void EmployeesLoad_NameWithInitialsCase()
		{
			NewSessionWithSameDB();
			var uowSaveSize = UnitOfWorkFactory.CreateWithoutRoot();
			InsertStandardSizes(uowSaveSize.Session.Connection);
			var nadejda = new EmployeeCard() {
				LastName = "Науменко",
				FirstName = "Надежда",
				Patronymic = "Александровна"
			};
			uowSaveSize.Save(nadejda);
			
			var elena = new EmployeeCard() {
				LastName = "Вершаловская", 
				FirstName = "Елена",
				Patronymic = "Владимировна"
			};
			uowSaveSize.Save(elena);
			uowSaveSize.Commit();

			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var dataParser = new DataParserEmployee(new PersonNames(), new SizeService(), new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated));
			var setting = new SettingsMatchEmployeesViewModel(null);
			var model = new ImportModelEmployee(dataParser, setting);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor))
			{
				var uow = employeesLoad.UoW;
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/Employees/name_with_initials.xlsx";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				model.Columns[1].DataTypeByLevels[0].DataType = model.DataTypes.First(x => DataTypeEmployee.NameWithInitials.Equals(x.Data)) ; //Вторая колонка Фамилия и инициалы.
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensitiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();
				uow.Commit();
				
				var employees = uow.GetAll<EmployeeCard>().ToList();

				Assert.That(employees.Count, Is.EqualTo(3));
				var employee1 = employees.First(x => x.LastName == "Науменко");
				Assert.That(employee1.FirstName, Is.EqualTo("Надежда")); // Имя не должны попортить
				Assert.That(employee1.Patronymic, Is.EqualTo("Александровна"));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("158-164"));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 2)?.Size?.Name, Is.EqualTo("60-62"));
				Assert.That(employee1.Sizes.FirstOrDefault(x => x.SizeType.Id == 4)?.Size?.Name, Is.EqualTo("42"));

				var employee2 = employees.First(x => x.LastName == "Вершаловская");
				Assert.That(employee2.FirstName, Is.EqualTo("Елена")); // Имя не должны попортить
				Assert.That(employee2.Patronymic, Is.EqualTo("Владимировна"));
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("158-164"));
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 2)?.Size?.Name, Is.EqualTo("48-50"));
				Assert.That(employee2.Sizes.FirstOrDefault(x => x.SizeType.Id == 4)?.Size?.Name, Is.EqualTo("37"));
				
				var employee3 = employees.First(x => x.LastName == "Чехонадский");
				Assert.That(employee3.FirstName, Is.EqualTo("Б")); // Заполняем только первые буквы, так как не знаем большего.
				Assert.That(employee3.Patronymic, Is.EqualTo("Н"));
				Assert.That(employee3.Sizes.FirstOrDefault(x => x.SizeType.Id == 1)?.Size?.Name, Is.EqualTo("194-200"));
				Assert.That(employee3.Sizes.FirstOrDefault(x => x.SizeType.Id == 2)?.Size?.Name, Is.EqualTo("68-70"));
				Assert.That(employee3.Sizes.FirstOrDefault(x => x.SizeType.Id == 4)?.Size?.Name, Is.EqualTo("46"));
			}
		}

		private void InsertStandardSizes(DbConnection connection) {
			string sql = @"
-- Преднастроенные типы размеров

INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (1,'Рост',1,'Height',1);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (2,'Размер одежды',1,'Size',2);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (4,'Размер обуви',1,'Size',4);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (5,'Размер зимней обуви',1,'Size',5);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (6,'Размер головного убора',1,'Size',6);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (7,'Размер перчаток',1,'Size',8);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (8,'Размер рукавиц',1,'Size',9);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (3,'Размер зимней одежды',0,'Size',3);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (9, 'Размер зимнего головного убора', 1, 'Size', 7);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (10, 'Размер противогаза', 1, 'Size', 10);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (11, 'Размер респиратора', 1, 'Size', 11);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (12, 'Размер носков', 1, 'Size', 12);

-- Преднастроенные размеры

INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (1, '146', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (2, '146-152', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (3, '152', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (4, '155-166', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (5, '158', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (6, '158-164', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (7, '164', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (8, '167-178', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (9, '170', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (10, '170-176', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (11, '176', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (12, '179-190', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (13, '182', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (14, '182-188', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (15, '188', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (16, '191-200', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (17, '194', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (18, '194-200', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (19, '200', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (20, '201-210', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (30, '38', 2, 1, 1, '76');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (31, '40', 2, 1, 1, '80');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (32, '40-42', 2, 0, 1, '80-84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (33, '42', 2, 1, 1, '84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (34, '44', 2, 1, 1, '88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (35, '44-46', 2, 0, 1, '88-92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (36, '46', 2, 1, 1, '92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (37, '48', 2, 1, 1, '96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (38, '48-50', 2, 0, 1, '96-100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (39, '50', 2, 1, 1, '100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (40, '50-52', 2, 0, 1, '100-104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (41, '52', 2, 1, 1, '104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (42, '52-54', 2, 0, 1, '104-108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (43, '54', 2, 1, 1, '108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (44, '56', 2, 1, 1, '112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (45, '56-58', 2, 0, 1, '112-116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (46, '58', 2, 1, 1, '116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (47, '58-60', 2, 0, 1, '116-120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (48, '60', 2, 1, 1, '120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (49, '60-62', 2, 0, 1, '120-124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (50, '62', 2, 1, 1, '124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (51, '62-64', 2, 0, 1, '124-128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (52, '64', 2, 1, 1, '128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (53, '64-66', 2, 0, 1, '128-132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (54, '66', 2, 1, 1, '132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (55, '68', 2, 1, 1, '136');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (56, '68-70', 2, 0, 1, '136-140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (57, '70', 2, 1, 1, '140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (58, '72', 2, 1, 1, '144');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (59, '72-74', 2, 0, 1, '144-148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (60, '74', 2, 1, 1, '148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (61, '74-76', 2, 0, 1, '148-152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (62, '76', 2, 1, 1, '152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (63, '76-78', 2, 0, 1, '152-156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (64, '78', 2, 1, 1, '156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (65, '80', 2, 1, 1, '160');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (66, '80-82', 2, 0, 1, '160-164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (67, '82', 2, 1, 1, '164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (68, 'XXS', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (69, 'XS', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (70, 'S', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (71, 'M', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (72, 'L', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (73, 'XL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (74, 'XXL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (75, 'XXXL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (76, '4XL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (77, '5XL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (78, '42-44', 2, 0, 1, '84-88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (79, '46-48', 2, 0, 1, '92-96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (80, '54-56', 2, 0, 1, '108-112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (90, '38', 3, 1, 1, '76');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (91, '40', 3, 1, 1, '80');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (92, '40-42', 3, 0, 1, '80-84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (93, '42', 3, 1, 1, '84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (94, '44', 3, 1, 1, '88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (95, '44-46', 3, 0, 1, '88-92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (96, '46', 3, 1, 1, '92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (97, '48', 3, 1, 1, '96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (98, '48-50', 3, 0, 1, '96-100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (99, '50', 3, 1, 1, '100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (100, '50-52', 3, 0, 1, '100-104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (101, '52', 3, 1, 1, '104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (102, '52-54', 3, 0, 1, '104-108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (103, '54', 3, 1, 1, '108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (104, '56', 3, 1, 1, '112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (105, '56-58', 3, 0, 1, '112-116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (106, '58', 3, 1, 1, '116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (107, '58-60', 3, 0, 1, '116-120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (108, '60', 3, 1, 1, '120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (109, '60-62', 3, 0, 1, '120-124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (110, '62', 3, 1, 1, '124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (111, '62-64', 3, 0, 1, '124-128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (112, '64', 3, 1, 1, '128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (113, '64-66', 3, 0, 1, '128-132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (114, '66', 3, 1, 1, '132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (115, '68', 3, 1, 1, '136');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (116, '68-70', 3, 0, 1, '136-140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (117, '70', 3, 1, 1, '140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (118, '72', 3, 1, 1, '144');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (119, '72-74', 3, 0, 1, '144-148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (120, '74', 3, 1, 1, '148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (121, '74-76', 3, 0, 1, '148-152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (122, '76', 3, 1, 1, '152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (123, '76-78', 3, 0, 1, '152-156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (124, '78', 3, 1, 1, '156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (125, '80', 3, 1, 1, '160');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (126, '80-82', 3, 0, 1, '160-164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (127, '82', 3, 1, 1, '164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (128, 'XXS', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (129, 'XS', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (130, 'S', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (131, 'M', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (132, 'L', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (133, 'XL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (134, 'XXL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (135, 'XXXL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (136, '4XL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (137, '5XL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (138, '42-44', 3, 0, 1, '84-88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (139, '46-48', 3, 0, 1, '92-96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (140, '54-56', 3, 0, 1, '108-112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (150, '34', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (151, '34-35', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (152, '35', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (153, '36', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (154, '36-37', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (155, '37', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (156, '38', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (157, '38-39', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (158, '39', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (159, '40', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (160, '40-41', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (161, '41', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (162, '42', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (163, '42-43', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (164, '43', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (165, '44', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (166, '44-45', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (167, '45', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (168, '46', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (169, '46-47', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (170, '47', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (171, '48', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (172, '49', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (173, '50', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (190, '34', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (191, '34-35', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (192, '35', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (193, '36', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (194, '36-37', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (195, '37', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (196, '38', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (197, '38-39', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (198, '39', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (199, '40', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (200, '40-41', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (201, '41', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (202, '42', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (203, '42-43', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (204, '43', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (205, '44', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (206, '44-45', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (207, '45', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (208, '46', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (209, '46-47', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (210, '47', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (211, '48', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (212, '49', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (213, '50', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (230, '54', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (231, '55', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (232, '56', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (233, '57', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (234, '58', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (235, '58-60', 6, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (236, '59', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (237, '59-60', 6, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (238, '60', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (239, '61', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (240, '62', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (241, '63', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (242, '64', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (243, '65', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (270, '6', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (271, '6,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (272, '7', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (273, '7,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (274, '8', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (275, '8,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (276, '8,5-9', 7, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (277, '9', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (278, '9,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (279, '10', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (280, '10,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (281, '11', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (282, '11,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (283, '12', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (284, '13', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (300, '1', 8, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (301, '2', 8, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (302, '3', 8, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (310, '54', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (311, '55', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (312, '56', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (313, '57', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (314, '58', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (315, '58-60', 9, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (316, '59', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (317, '59-60', 9, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (318, '60', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (319, '61', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (320, '62', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (321, '63', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (322, '64', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (323, '65', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (330, '1', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (331, '2', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (332, '3', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (333, '4', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (340, '1', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (341, '2', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (342, '3', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (343, '4', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (350, '23', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (351, '23-25', 12, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (352, '25', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (353, '27', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (354, '27-29', 12, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (355, '29', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (356, '31', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (357, '31-33', 12, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (358, '33', 12, 1, 0, NULL);";
			connection.Execute(sql);
		}
	}
}
