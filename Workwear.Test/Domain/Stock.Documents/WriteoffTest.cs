using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Test.Domain.Stock.Documents
{
	[TestFixture(TestOf = typeof(Writeoff))]
	public class WriteoffTest
	{
		[Test(Description = "Мы должны иметь возможность изменять процент износа.")]
		public void UpdateOperations_WarehouseOperation_CanChangeWearPercentTest()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var nomenclature = Substitute.For<Nomenclature>();

			var warehouse = Substitute.For<Warehouse>();

			var position = new StockPosition(nomenclature, 0.2m, new Size(), new Size(), null);

			var writeoff = new Writeoff {
				Date = new DateTime(2019, 1, 15)
			};
			writeoff.AddItem(position, warehouse, 2);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(0.2m));
			//Меняем значение процента износа
			writeoff.Items[0].WearPercent = 2;

			//Выполняем
			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(2));
		}

		[Test(Description = "Мы должны иметь возможность изменять процент износа.")]
		public void UpdateOperations_EmployeeIssueOperation_CanChangeWearPercentTest()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var nomenclature = Substitute.For<Nomenclature>();

			var employee = Substitute.For<EmployeeCard>();

			var issueOperation = new EmployeeIssueOperation {
				OperationTime = new DateTime(2019, 1, 1),
				StartOfUse = new DateTime(2019, 1, 1),
				Issued = 10,
				Nomenclature = nomenclature,
				WearPercent = 0,
				ExpiryByNorm = new DateTime(2019, 1, 15),
				Employee = employee
			};

			var writeoff = new Writeoff {
				Date = new DateTime(2019, 1, 15)
			};
			writeoff.AddItem(issueOperation, 2);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(1));
			//Меняем значение процента износа
			writeoff.Items[0].WearPercent = 2;

			//Выполняем
			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(2));
		}

		[Test(Description = "Операция списания с дежурной нормы должна ссылаться на правильную строку нормы")]
		public void UpdateOperations_DutyNormIssueOperation_ChildOperationInheritsDutyNormItem()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var protectionTools = new ProtectionTools();
			var dutyNormItem = new DutyNormItem { ProtectionTools = protectionTools, Amount = 2 };
			var issueOperation = new DutyNormIssueOperation {
				DutyNorm = new DutyNorm(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = protectionTools,
				DutyNormItem = dutyNormItem,
				Issued = 2
			};

			var writeoff = new Writeoff { Date = new DateTime(2019, 1, 15) };
			writeoff.AddItem(issueOperation, 1);

			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].DutyNormWriteOffOperation.DutyNormItem, Is.SameAs(dutyNormItem));
		}

		[Test(Description = "Списание с дежурной нормы должно уменьшать числящееся.")]
		public void UpdateOperations_DutyNormIssueOperation_DecreasesIssuedAmountAndUpdatesNextIssue()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var dutyNorm = new DutyNorm();
			var protectionTools = new ProtectionTools();
			var dutyNormItem = new DutyNormItem { DutyNorm = dutyNorm, ProtectionTools = protectionTools, Amount = 2 };
			var issueOperation = new DutyNormIssueOperation {
				DutyNorm = dutyNorm,
				Nomenclature = new Nomenclature(),
				ProtectionTools = protectionTools,
				DutyNormItem = dutyNormItem,
				OperationTime = new DateTime(2019, 1, 1),
				Issued = 2
			};

			dutyNormItem.Graph = new IssueGraph(new List<IGraphIssueOperation> { issueOperation });
			dutyNormItem.UpdateNextIssue();
			Assert.That(dutyNormItem.Issued(new DateTime(2019, 1, 1)), Is.EqualTo(2));
			Assert.That(dutyNormItem.NextIssue, Is.Null);

			var writeoff = new Writeoff { Date = new DateTime(2019, 1, 15) };
			writeoff.AddItem(issueOperation, 1);
			writeoff.UpdateOperations(uow);

			dutyNormItem.Graph = new IssueGraph(new List<IGraphIssueOperation> {
				issueOperation, writeoff.Items[0].DutyNormWriteOffOperation
			});
			dutyNormItem.UpdateNextIssue();

			Assert.That(dutyNormItem.Issued(new DateTime(2019, 1, 15)), Is.EqualTo(1),
				"Числящееся должно уменьшиться на списанное количество.");
			Assert.That(dutyNormItem.NextIssue, Is.EqualTo(new DateTime(2019, 1, 15)),
				"После списания выданного недостаточно для нормы - должна появиться дата следующей выдачи.");
		}
	}
}
