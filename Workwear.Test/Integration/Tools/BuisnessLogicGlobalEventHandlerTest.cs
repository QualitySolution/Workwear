using System;
using System.Linq;
using System.Threading;
using Dapper;
using NSubstitute;
using NUnit.Framework;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.Test.Integration.Tools
{
	[TestFixture(TestOf = typeof(BusinessLogicGlobalEventHandler))]
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

		[Test(Description = "Проверяем что после удаления пересчитываем даты и следующею выдачу")]
		public void HandleDeleteEmployeeVacation_RecalculateDatesAndNextIssueTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
            baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Тест на обработку события удаления")) {
				MakeBaseParametersTable(uow);
				BusinessLogicGlobalEventHandler.Init(ask, UnitOfWorkFactory);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
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

				var warehouseOperation = new WarehouseOperation();
				warehouseOperation.Nomenclature = nomenclature;
				uow.Save(warehouseOperation);

				var expenseOp = new EmployeeIssueOperation();
				expenseOp.OperationTime = new DateTime(2019, 1, 1);
				expenseOp.ExpiryByNorm = new DateTime(2019, 4, 1);
				expenseOp.Employee = employee;
				expenseOp.Nomenclature = nomenclature;
				expenseOp.ProtectionTools = protectionTools;
				expenseOp.NormItem = normItem;
				expenseOp.Issued = 1;
				expenseOp.WarehouseOperation = warehouseOperation;
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

		[Test(Description = "Проверяем что после удаления пересчитываем правильно даты с двумя одинаковыми выдачами за день, " +
		                    "расположенными в неправильном порядке.")]
		[Category("Real case")]
		public void HandleDeleteEmployeeVacation_RecalculateWithTwoIssuePerDayTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Тест на обработку события удаления")) {
				MakeBaseParametersTable(uow);
				BusinessLogicGlobalEventHandler.Init(ask, UnitOfWorkFactory);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);

				var vacationType = new VacationType();
				vacationType.Name = "Тестовый отпуск";
				vacationType.ExcludeFromWearing = true;

				var vacation = new EmployeeVacation();
				vacation.BeginDate = new DateTime(2019, 2, 1);
				vacation.EndDate = new DateTime(2019, 2, 10);
				vacation.VacationType = vacationType;
				employee.AddVacation(vacation);
				uow.Save(vacationType);
				uow.Save(vacation);
				uow.Save(employee);
				uow.Commit();

				var warehouseOperation = new WarehouseOperation();
				warehouseOperation.Nomenclature = nomenclature;
				uow.Save(warehouseOperation);

				var expenseOp = new EmployeeIssueOperation();
				expenseOp.OperationTime = new DateTime(2019, 1, 1, 14, 0, 0);
				expenseOp.AutoWriteoffDate = new DateTime(2020, 1, 1);
				expenseOp.Employee = employee;
				expenseOp.Nomenclature = nomenclature;
				expenseOp.ProtectionTools = protectionTools;
				expenseOp.NormItem = normItem;
				expenseOp.Issued = 1;
				expenseOp.WarehouseOperation = warehouseOperation;
				var graph = IssueGraph.MakeIssueGraph(uow, employee, protectionTools);
				expenseOp.RecalculateDatesOfIssueOperation(graph, baseParameters, ask);
				uow.Save(expenseOp);

				var warehouseOperation2 = new WarehouseOperation();
				warehouseOperation2.Nomenclature = nomenclature;
				uow.Save(warehouseOperation2);

				var expenseOp2 = new EmployeeIssueOperation();
				expenseOp2.OperationTime = new DateTime(2019, 1, 1, 13, 0, 0);
				expenseOp2.AutoWriteoffDate = new DateTime(2020, 1, 1);
				expenseOp2.Employee = employee;
				expenseOp2.Nomenclature = nomenclature;
				expenseOp2.ProtectionTools = protectionTools;
				expenseOp2.NormItem = normItem;
				expenseOp2.Issued = 1;
				expenseOp2.WarehouseOperation = warehouseOperation2;
				graph = IssueGraph.MakeIssueGraph(uow, employee, protectionTools);
				expenseOp2.RecalculateDatesOfIssueOperation(graph, baseParameters, ask);
				uow.Save(expenseOp2);
				uow.Commit();

				employee.RecalculateDatesOfIssueOperations(uow, new EmployeeIssueRepository(), baseParameters, ask, vacation);
				uow.Commit();

				Assert.That(employee.WorkwearItems[0].NextIssue, Is.EqualTo(new DateTime(2021, 1, 11)));

				//Выполняем удаление
				employee.Vacations.Remove(vacation);
				uow.Delete(vacation);
				uow.Commit();

				//проверяем данные
				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot("Тест на обработку события удаления uow2")) {
					var resultEmployee = uow2.GetById<EmployeeCard>(employee.Id);
					Assert.That(resultEmployee.WorkwearItems[0].NextIssue, Is.EqualTo(new DateTime(2021, 1, 1)));
				}

			}
		}

		[Test(Description = "Корректно удаляем строку из документа выдачи сотруднику. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_DeleteRowTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				BusinessLogicGlobalEventHandler.Init(ask, UnitOfWorkFactory);

				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, 0, null, null, null);

				var nomenclatureType2 = new ItemsType();
				nomenclatureType2.Name = "Тестовый тип номенклатуры2";
				uow.Save(nomenclatureType2);

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType2;
				uow.Save(nomenclature2);

				var protectionTools1 = new ProtectionTools();
				protectionTools1.Name = "СИЗ для тестирования";
				protectionTools1.AddNomeclature(nomenclature);
				uow.Save(protectionTools1);

				var protectionTools2 = new ProtectionTools();
				protectionTools2.Name = "СИЗ для тестирования2";
				protectionTools2.AddNomeclature(nomenclature2);
				uow.Save(protectionTools2);

				var position2 = new StockPosition(nomenclature2, 0, null, null, null);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools1);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				var normItem2 = norm.AddItem(protectionTools2);
				normItem2.Amount = 1;
				normItem2.NormPeriod = NormPeriodType.Month;
				normItem2.PeriodCount = 4;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.HireDate = new DateTime(2018, 1, 15);
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2, ask);
				incomeItem2.Amount = 10;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense();
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 4, 22);
				expense.Operation = ExpenseOperations.Employee;
				expense.AddItem(position1, 1);
				expense.AddItem(position2, 1);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();
				expense.UpdateEmployeeWearItems();
				employee.WorkwearItems.First(e => e.ProtectionTools.IsSame(protectionTools2)).Created = new DateTime(2018, 4, 22);
				uow.Save(expense.Employee);
				uow.Commit();

				Assert.That(employee.WorkwearItems.First(e => e.ProtectionTools.IsSame(protectionTools2)).NextIssue,
					Is.EqualTo(new DateTime(2018, 8, 22))
				);

				//Выполняем удаление
				expense.RemoveItem(expense.Items.First(e => e.Nomenclature.Type.IsSame(nomenclatureType2)));
				uow.Save(expense);
				uow.Commit();

				//проверяем данные
				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot("Тест на обработку события удаления uow2")) {
					var resultEmployee = uow2.GetById<EmployeeCard>(employee.Id);
					Assert.That(resultEmployee.WorkwearItems.First(e => e.ProtectionTools.IsSame(protectionTools2)).NextIssue, 
					Is.EqualTo(new DateTime(2018, 4, 22)));
				}
			}
		}

		[Test(Description = "Проверяем что не падаем при удаления сотрудника")]
		[Category("real case")]
		public void HandleDelete_CanDeleteEmployeeTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Тест на обработку удаления сотрудника")) {
				BusinessLogicGlobalEventHandler.Init(ask, UnitOfWorkFactory);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 2;
				uow.Save(norm);

				var employee = new EmployeeCard();
				uow.Save(employee);

				var warehouseOperation = new WarehouseOperation();
				var expenseOp = new EmployeeIssueOperation();
				expenseOp.OperationTime = warehouseOperation.OperationTime = new DateTime(2019, 1, 1);
				expenseOp.ExpiryByNorm = new DateTime(2019, 4, 1);
				expenseOp.ProtectionTools = protectionTools;
				expenseOp.Employee = employee;
				expenseOp.Nomenclature = warehouseOperation.Nomenclature = nomenclature;
				expenseOp.NormItem = normItem;
				warehouseOperation.Amount = expenseOp.Issued = 1;
				expenseOp.WarehouseOperation = warehouseOperation;
				uow.Save(nomenclature);
				uow.Save(normItem);
				uow.Save(warehouseOperation);
				uow.Save(expenseOp);
				uow.Commit();

				//FIXME Временно чтобы переделка не вызвала конфликт мержа в 2.4
				Configure.ConfigureDeletion();
				var deletion = new DeleteCore(DeleteConfig.Main, uow);
				deletion.PrepareDeletion(typeof(EmployeeCard), employee.Id, CancellationToken.None);
				deletion.RunDeletion(CancellationToken.None);
			}
		}

		#region Helpers

		private void MakeBaseParametersTable(IUnitOfWork uow) {
			var sql = @"CREATE TABLE IF NOT EXISTS `base_parameters` (
				`name` VARCHAR(20) NOT NULL,
				`str_value` VARCHAR(100) NULL DEFAULT NULL,
				PRIMARY KEY (`name`));";
			uow.Session.Connection.Execute(sql);
		}

		#endregion
	}
}
