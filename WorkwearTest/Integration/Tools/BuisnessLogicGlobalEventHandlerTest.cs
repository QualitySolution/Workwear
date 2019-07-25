using NSubstitute;
using NUnit.Framework;
using QS.Testing.DB;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using System;
using workwear.Domain.Operations;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Tools;

namespace WorkwearTest.Integration.Tools
{
	[TestFixture(TestOf = typeof(BuisnessLogicGlobalEventHandler))]
	[Category("Integrated")]
	public class BuisnessLogicGlobalEventHandlerTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
			NotifyConfiguration.Enable();
		}

		[SetUp]
		public void TestSetup()
		{
			NewSessionWithSameDB();
		}

		[TearDown]
		public void TestTearDown()
		{
			NewSessionWithNewDB();
		}

		[Test()]
		public void HandleDeleteEmployeeVacation_RecalculateDatesAndNextIssueTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Тест на обработку события удаления")) {
				BuisnessLogicGlobalEventHandler.Init(ask, UnitOfWorkFactory);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var norm = new Norm();
				var normItem = norm.AddItem(nomenclatureType);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 2;
				uow.Save(norm);

				var employee = new EmployeeCard();

				var vacationType = new VacationType();
				vacationType.Name = "Тестовый отпуск";
				vacationType.ExcludeFromWearing = true;

				var vacation = new EmployeeVacation();
				vacation.BeginDate = new DateTime(2019, 2, 1);
				vacation.EndDate = new DateTime(2019, 3, 1);
				vacation.VacationType = vacationType;
				employee.AddVacation(vacation);
				uow.Save(vacationType);
				uow.Save(vacation);
				uow.Save(employee);

				var expenseOp = new EmployeeIssueOperation();
				expenseOp.OperationTime = new DateTime(2019, 1, 1);
				expenseOp.ExpiryByNorm = new DateTime(2019, 4, 1);
				expenseOp.Employee = employee;
				expenseOp.Nomenclature = nomenclature;
				expenseOp.NormItem = normItem;
				expenseOp.Issued = 1;
				uow.Save(nomenclature);
				uow.Save(normItem);
				uow.Save(expenseOp);
				uow.Commit();

				//Выполняем удаление
				employee.Vacations.Remove(vacation);
				uow.Delete(vacation);
				uow.Commit();

				//проверяем данные
				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot("Тест на обработку события удаления uow2")) {
					var resultOp = uow2.GetById<EmployeeIssueOperation>(expenseOp.Id);
					Assert.That(resultOp.ExpiryByNorm, Is.EqualTo(new DateTime(2019, 3, 1)));
				}

			}
		}
	}
}
