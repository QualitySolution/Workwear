using System;
using System.Linq;
using NUnit.Framework;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Test.Integration.Operations
{
	[TestFixture(TestOf = typeof(IssueGraph), Description = "Граф числящегося за сотрудником")]
	public class IssueGraphIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что в графе появляются вручную созданные операции без номенклатуры.")]
		[Category("Integrated")]
		public void MakeIssueGraph_UseManualOperationsTest()
		{

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

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

				var employee = new EmployeeCard();
				uow.Save(employee);

				//Операция без номенклатуры
				var manualOp = new EmployeeIssueOperation();
				manualOp.OperationTime = new DateTime(2019, 1, 1, 14, 0, 0);
				manualOp.AutoWriteoffDate = new DateTime(2020, 1, 1);
				manualOp.Employee = employee;
				manualOp.ProtectionTools = protectionTools;
				manualOp.Issued = 1;
				manualOp.OverrideBefore = true;
				uow.Save(manualOp);

				var expenseOp = new EmployeeIssueOperation();
				expenseOp.OperationTime = new DateTime(2020, 1, 1, 13, 0, 0);
				expenseOp.AutoWriteoffDate = new DateTime(2021, 1, 1);
				expenseOp.Employee = employee;
				expenseOp.Nomenclature = nomenclature;
				expenseOp.ProtectionTools = protectionTools;
				expenseOp.Issued = 1;
				uow.Save(expenseOp);

				uow.Commit();

				var graph = IssueGraph.MakeIssueGraph(uow, employee, protectionTools);
				Assert.That(graph.Intervals.Count, Is.EqualTo(3));
				var first = graph.OrderedIntervals.First();
				Assert.That(first.StartDate, Is.EqualTo(new DateTime(2019, 1, 1)));
			}
		}
	}
}
