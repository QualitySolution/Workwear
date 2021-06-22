using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Navigation;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Tools.Import;
using workwear.ViewModels.Tools;
using Workwear.Domain.Company;

namespace WorkwearTest.Integration.Tools
{
	[TestFixture(TestOf = typeof(EmployeesLoadViewModel))]
	[Category("Integrated")]
	public class EmployeesLoadViewModelTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что без проблем можем загрузить файл формата со Стойленского ГОК")]
		public void EmployeesLoadStolenskyGok()
		{
			var navigation = Substitute.For<INavigationManager>();
			var interactive = Substitute.For<IInteractiveMessage>();
			var progressStep3 = Substitute.For<IProgressBarDisplayable>();
			var dataparser = new DataParser();
			using(var employeesLoad = new EmployeesLoadViewModel(UnitOfWorkFactory, navigation, interactive, dataparser)) {
				employeesLoad.ProgressStep3 = progressStep3;
				employeesLoad.FileName = "cardkey_list.xlsx";
				Assert.That(employeesLoad.Sheets.Count, Is.GreaterThan(0));
				employeesLoad.SelectedSheet = employeesLoad.Sheets.First();
				Assert.That(employeesLoad.SensetiveSecondStepButton, Is.True, "Кнопка второго шага должна быть доступна");
				employeesLoad.SecondStep();
				Assert.That(employeesLoad.SensetiveThirdStepButton, Is.True, "Кнопка третьего шага должна быть доступна");
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
	}
}
