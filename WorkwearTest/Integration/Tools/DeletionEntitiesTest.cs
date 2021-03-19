using System;
using System.Linq;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using QS.Deletion;
using QS.Dialog;
using QS.Navigation;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Tools;

namespace WorkwearTest.Integration.Tools
{
	[TestFixture(Description = "Различные тесты на реальное удаление из базы различных сущьностей.")]
	[Category("Integrated")]
	public class DeletionEntitiesTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
			ConfigureOneTime.ConfigureDeletion();
		}

		[Test(Description = "Проверяем что можем удалить созданный документ выдачи.")]
		[Category("Integrated")]
		public void Deletion_ExpenseEmployeeDocumentTest()
		{
			NewSessionWithSameDB();
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var navigation = Substitute.For<INavigationManager>();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, null, null, 0);

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType;
				uow.Save(nomenclature2);

				var position2 = new StockPosition(nomenclature2, null, null, 0);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var protectionTools2 = new ProtectionTools();
				protectionTools2.Name = "СИЗ для тестирования 2";
				protectionTools2.AddNomeclature(nomenclature2);
				uow.Save(protectionTools2);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				var normItem2 = norm.AddItem(protectionTools2);
				normItem2.Amount = 1;
				normItem2.NormPeriod = NormPeriodType.Month;
				normItem2.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense();
				expense.Operation = ExpenseOperations.Employee;
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				expense.AddItem(position1, 1);
				expense.AddItem(position2, 1);

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.ColDayAheadOfShedule.Returns(0);

				expense.CreateIssuanceSheet();

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Save(expense.IssuanceSheet);
				uow.Commit();

				expense.UpdateEmployeeWearItems();
				uow.Commit();

				var docs = uow.GetAll<Expense>().ToList();
				Assert.That(docs.Count, Is.EqualTo(1));

				//Непосредственно удаление документа
				var cancel = new CancellationTokenSource();
				using(var uowDel = UnitOfWorkFactory.CreateWithoutRoot()) {
					var deletionService = new DeleteCore(DeleteConfig.Main, uowDel);
					deletionService.PrepareDeletion(typeof(Expense), expense.Id, cancel.Token);
					Assert.That(deletionService.TotalLinks, Is.GreaterThan(0));
					deletionService.RunDeletion(cancel.Token);
				}

				//Проверяем удаление
				var expenseId = expense.Id;
				using(var uowCheck = UnitOfWorkFactory.CreateWithoutRoot()) {
					//Проверяем что удалили документ.
					docs = uow.GetAll<Expense>().ToList();
					Assert.That(docs.Count, Is.Zero);

					//Проверяем что случайно не удалили СИЗ и номеклатуру.
					var protections = uow.GetAll<ProtectionTools>().ToList();
					Assert.That(protections.Count, Is.EqualTo(2));
					var nomenclatures = uow.GetAll<Nomenclature>().ToList();
					Assert.That(nomenclatures.Count, Is.EqualTo(2));
				}
			}
		}
	}
}
