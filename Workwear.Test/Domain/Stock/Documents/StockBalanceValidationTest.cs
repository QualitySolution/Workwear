using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;
using Workwear.Models.Operations;
using Workwear.Tools;

namespace Workwear.Test.Domain.Stock.Documents
{
	[TestFixture]
	public class StockBalanceValidationTest
	{
		private static readonly DateTime DocumentDate = new DateTime(2026, 8, 31);

		[Test(Description = "Сохраненную выдачу сотруднику можно повторно сохранить при остатке, равном количеству выдачи.")]
		public void Expense_ValidateSavedDocumentWithExactBalance_ReturnsNoErrors()
		{
			var warehouse = new Warehouse();
			var nomenclature = new Nomenclature();
			var operation = new WarehouseOperation { Id = 1, Nomenclature = nomenclature, Amount = 1 };
			var document = new Expense { Warehouse = warehouse, Employee = new EmployeeCard(), Date = DocumentDate };
			document.Items.Add(new ExpenseItem {
				Id = 1, ExpenseDoc = document, Nomenclature = nomenclature, Amount = 1, WarehouseOperation = operation
			});

			AssertValidAndOwnOperationExcluded(document, warehouse, nomenclature, operation);
		}

		[Test(Description = "Сохраненную коллективную выдачу можно повторно сохранить при остатке, равном количеству выдачи.")]
		public void CollectiveExpense_ValidateSavedDocumentWithExactBalance_ReturnsNoErrors()
		{
			var warehouse = new Warehouse();
			var nomenclature = new Nomenclature();
			var operation = new WarehouseOperation { Id = 1, Nomenclature = nomenclature, Amount = 1 };
			var document = new CollectiveExpense { Warehouse = warehouse, Date = DocumentDate };
			document.Items.Add(new CollectiveExpenseItem {
				Id = 1, Document = document, Nomenclature = nomenclature, Amount = 1, WarehouseOperation = operation
			});

			AssertValidAndOwnOperationExcluded(document, warehouse, nomenclature, operation);
		}

		[Test(Description = "Сохраненную выдачу по дежурной норме можно повторно сохранить при остатке, равном количеству выдачи.")]
		public void ExpenseDutyNorm_ValidateSavedDocumentWithExactBalance_ReturnsNoErrors()
		{
			var warehouse = new Warehouse();
			var nomenclature = new Nomenclature();
			var operation = new WarehouseOperation { Id = 1, Nomenclature = nomenclature, Amount = 1 };
			var document = new ExpenseDutyNorm { Warehouse = warehouse, DutyNorm = new DutyNorm(), Date = DocumentDate };
			document.Items.Add(new ExpenseDutyNormItem {
				Id = 1,
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = nomenclature,
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				},
				WarehouseOperation = operation
			});

			AssertValidAndOwnOperationExcluded(document, warehouse, nomenclature, operation);
		}

		[Test(Description = "Сохраненную выдачу вне нормы можно повторно сохранить при остатке, равном количеству выдачи.")]
		public void OverNorm_ValidateSavedDocumentWithExactBalance_ReturnsNoErrors()
		{
			var warehouse = new Warehouse();
			var nomenclature = new Nomenclature();
			var operation = new WarehouseOperation { Id = 1, Nomenclature = nomenclature, Amount = 1 };
			var document = new OverNorm { Warehouse = warehouse, Date = DocumentDate };
			document.AddItem(new OverNormOperation {
				Employee = new EmployeeCard(),
				WarehouseOperation = operation
			});

			AssertValidAndOwnOperationExcluded(document, warehouse, nomenclature, operation);
		}

		[Test(Description = "Сохраненное перемещение можно повторно сохранить при остатке, равном количеству перемещения.")]
		public void Transfer_ValidateSavedDocumentWithExactBalance_ReturnsNoErrors()
		{
			var warehouseFrom = new Warehouse();
			var warehouseTo = new Warehouse();
			var nomenclature = new Nomenclature();
			var document = new Transfer {
				WarehouseFrom = warehouseFrom,
				WarehouseTo = warehouseTo,
				Date = DocumentDate
			};
			var item = document.AddItem(new StockPosition(nomenclature, 0, null, null, null), 1);
			item.Id = 1;
			item.WarehouseOperation.Id = 1;

			var repository = Substitute.For<StockRepository>();
			repository.StockBalances(
				warehouseFrom,
				Arg.Any<IEnumerable<Nomenclature>>(),
				DocumentDate,
				Arg.Any<IEnumerable<WarehouseOperation>>())
				.Returns(new List<StockBalanceDTO> { new StockBalanceDTO { Nomenclature = nomenclature, Amount = 1 } });
			var balances = new StockBalanceModel(new UnitOfWorkProvider(), repository) {
				Warehouse = warehouseFrom,
				OnDate = DocumentDate,
				ExcludeOperations = new[] { item.WarehouseOperation }
			};
			balances.AddNomenclatures(new[] { nomenclature });
			item.StockBalanceModel = balances;

			var parameters = Substitute.For<BaseParameters>();
			parameters.CheckBalances.Returns(true);
			var errors = document.Validate(new ValidationContext(document, null, new Dictionary<object, object> {
				{ nameof(BaseParameters), parameters }
			})).ToList();

			Assert.That(errors, Is.Empty);
			repository.Received().StockBalances(
				warehouseFrom,
				Arg.Any<IEnumerable<Nomenclature>>(),
				DocumentDate,
				Arg.Is<IEnumerable<WarehouseOperation>>(operations => operations.Single() == item.WarehouseOperation));
		}

		[Test(Description = "Сохраненное списание можно повторно сохранить при остатке, равном количеству списания.")]
		public void Writeoff_ValidateSavedDocumentWithExactBalance_ReturnsNoErrors()
		{
			var nomenclature = new Nomenclature();
			var document = new Writeoff { Date = DocumentDate };
			var item = document.AddItem(new StockPosition(nomenclature, 0, null, null, null), new Warehouse(), 1);
			item.Id = 1;
			item.WarehouseOperation.Id = 1;
			item.MaxAmount = 1;

			var parameters = Substitute.For<BaseParameters>();
			parameters.CheckBalances.Returns(true);
			var errors = document.Validate(new ValidationContext(document, null, new Dictionary<object, object> {
				{ nameof(BaseParameters), parameters }
			})).ToList();

			Assert.That(errors, Is.Empty);
		}

		private static void AssertValidAndOwnOperationExcluded(
			IValidatableObject document,
			Warehouse warehouse,
			Nomenclature nomenclature,
			WarehouseOperation ownOperation)
		{
			var parameters = Substitute.For<BaseParameters>();
			parameters.CheckBalances.Returns(true);
			var repository = Substitute.For<StockRepository>();
			repository.StockBalances(
				warehouse,
				Arg.Any<IEnumerable<Nomenclature>>(),
				DocumentDate,
				Arg.Any<IEnumerable<WarehouseOperation>>())
				.Returns(new List<StockBalanceDTO> { new StockBalanceDTO { Nomenclature = nomenclature, Amount = 1 } });

			var errors = document.Validate(new ValidationContext(document, null, new Dictionary<object, object> {
				{ nameof(BaseParameters), parameters },
				{ nameof(StockRepository), repository }
			})).ToList();

			Assert.That(errors, Is.Empty);
			repository.Received().StockBalances(
				warehouse,
				Arg.Any<IEnumerable<Nomenclature>>(),
				DocumentDate,
				Arg.Is<IEnumerable<WarehouseOperation>>(operations => operations.Single() == ownOperation));
		}
	}
}
