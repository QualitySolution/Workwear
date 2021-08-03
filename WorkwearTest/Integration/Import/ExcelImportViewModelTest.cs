using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Navigation;
using QS.Testing.DB;
using workwear.Domain.Company;
using Workwear.Domain.Company;
using workwear.Models.Import;
using workwear.Repository.Company;
using workwear.Tools.Nhibernate;
using workwear.ViewModels.Import;

namespace WorkwearTest.Integration.Tools
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

		[Test(Description = "Проверяем что без проблем можем загрузить файл формата со Стойленского ГОК")]
		public void EmployeesLoad_StolenskyGok()
		{
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var subdivisionRepository = Substitute.For<SubdivisionRepository>();
			var postRepository = Substitute.For<PostRepository>();
			var dataparser = new DataParserEmployee(subdivisionRepository, postRepository);
			var model = new ImportModelEmployee(dataparser);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/cardkey_list.xlsx";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensetiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
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
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep = Substitute.For<IProgressBarDisplayable>();
			var progressInterceptor = Substitute.For<ProgressInterceptor>();
			var subdivisionRepository = Substitute.For<SubdivisionRepository>();
			var postRepository = Substitute.For<PostRepository>();
			var dataparser = new DataParserEmployee(subdivisionRepository, postRepository);
			var model = new ImportModelEmployee(dataparser);
			using(var employeesLoad = new ExcelImportViewModel(model, UnitOfWorkFactory, navigation, interactive, progressInterceptor)) {
				employeesLoad.ProgressStep = progressStep;
				employeesLoad.FileName = "Samples/Excel/vostok_1c_employee.xls";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensitiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensitiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
				employeesLoad.ThirdStep();
				Assert.That(employeesLoad.SensetiveSaveButton, Is.True, "Кнопка сохранить должна быть доступна");
				employeesLoad.Save();

				var uow = employeesLoad.UoW;
				var employees = uow.GetAll<EmployeeCard>().ToList();
				Assert.That(employees.Count, Is.EqualTo(4));
				var olga = employees.First(x => x.PersonnelNumber == "00001");
				Assert.That(olga.LastName, Is.EqualTo("Гриднева"));
				Assert.That(olga.FirstName, Is.EqualTo("Ольга"));
				Assert.That(olga.Patronymic, Is.EqualTo("Николаевна"));
				Assert.That(olga.HireDate, Is.EqualTo(new DateTime(2020, 3, 10)));
				Assert.That(olga.Sex, Is.EqualTo(Sex.F));
				Assert.That(olga.Subdivision.Name, Is.EqualTo("500006 Отдел главного энергетика"));
				Assert.That(olga.Post.Name, Is.EqualTo("Ведущий инженер"));
				Assert.That(olga.Post.Subdivision.Name, Is.EqualTo("500006 Отдел главного энергетика"));
				
				//Проверяем что должности из разных подразделений не сливаются.
				var natalia = employees.First(x => x.PersonnelNumber == "00002");
				Assert.That(natalia.Subdivision.Name, Is.EqualTo("500007 Отдел главного механика"));
				Assert.That(natalia.Post.Name, Is.EqualTo("Ведущий инженер"));
				Assert.That(natalia.Post.Subdivision.Name, Is.EqualTo("500007 Отдел главного механика"));
				
				//Проверяем что не дублируем должности и подразделения.
				var igor = employees.First(x => x.PersonnelNumber == "00003");
				var ury = employees.First(x => x.PersonnelNumber == "00005");
				Assert.That(igor.Subdivision.Name, Is.EqualTo("500300 Цех санитарных керамических изделий"));
				Assert.That(igor.Post.Name, Is.EqualTo("Изготовитель капов (из эпоксидной смолы)."));
				Assert.That(igor.Post.Subdivision.Name, Is.EqualTo("500300 Цех санитарных керамических изделий"));
				Assert.That(igor.Subdivision.Id, Is.EqualTo(ury.Subdivision.Id));
				Assert.That(igor.Post.Id, Is.EqualTo(ury.Post.Id));
				Assert.That(igor.Post.Subdivision.Id, Is.EqualTo(ury.Post.Subdivision.Id));
			}
		}
	}
}
