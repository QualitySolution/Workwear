using System.Linq;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Test.Domain.Stock.Documents {
	[TestFixture(TestOf = typeof(ReturnItem))]
	public class ReturnItemTest {
		[Test(Description = "При возврате вне нормы с выбранными штрихкодами в операцию возврата попадают только выбранные штрихкоды.")]
		public void Constructor_OverNormWithSelectedBarcode_AddOnlySelectedBarcodeOperation()
		{
			var selectedBarcode = new Barcode { Title = "100001" };
			var otherBarcode = new Barcode { Title = "100002" };
			var issuedOperation = CreateOverNormOperation(selectedBarcode, otherBarcode);
			var document = new Return {
				Warehouse = new Warehouse()
			};

			var item = new ReturnItem(document, issuedOperation, 1, barcodes: new[] { selectedBarcode });

			Assert.That(item.ReturnFromOverNormOperation.BarcodeOperations, Has.Count.EqualTo(1));
			Assert.That(
				item.ReturnFromOverNormOperation.BarcodeOperations.Single().Barcode,
				Is.SameAs(selectedBarcode));
			Assert.That(item.BarcodesString, Is.EqualTo(selectedBarcode.Title));
			Assert.That(item.CanEditAmount, Is.False);
		}

		[Test(Description = "При возврате от сотрудника с выбранными штрихкодами в операцию возврата попадают только выбранные штрихкоды.")]
		public void Constructor_EmployeeIssueWithSelectedBarcode_AddOnlySelectedBarcodeOperation()
		{
			var selectedBarcode = new Barcode { Title = "100001" };
			var otherBarcode = new Barcode { Title = "100002" };
			var issuedOperation = CreateEmployeeIssueOperation(selectedBarcode, otherBarcode);
			var document = new Return();

			var item = new ReturnItem(document, issuedOperation, 1, new[] { selectedBarcode });

			Assert.That(item.ReturnFromEmployeeOperation.BarcodeOperations, Has.Count.EqualTo(1));
			Assert.That(
				item.ReturnFromEmployeeOperation.BarcodeOperations.Single().Barcode,
				Is.SameAs(selectedBarcode));
			Assert.That(item.BarcodesString, Is.EqualTo(selectedBarcode.Title));
			Assert.That(item.CanEditAmount, Is.False);
		}

		[Test(Description = "При возврате с дежурной нормы с выбранными штрихкодами в операцию возврата попадают только выбранные штрихкоды.")]
		public void Constructor_DutyNormIssueWithSelectedBarcode_AddOnlySelectedBarcodeOperation()
		{
			var selectedBarcode = new Barcode { Title = "100001" };
			var otherBarcode = new Barcode { Title = "100002" };
			var issuedOperation = CreateDutyNormIssueOperation(selectedBarcode, otherBarcode);
			var document = new Return();

			var item = new ReturnItem(document, issuedOperation, 1, new[] { selectedBarcode });

			Assert.That(item.ReturnFromDutyNormOperation.BarcodeOperations, Has.Count.EqualTo(1));
			Assert.That(
				item.ReturnFromDutyNormOperation.BarcodeOperations.Single().Barcode,
				Is.SameAs(selectedBarcode));
			Assert.That(item.BarcodesString, Is.EqualTo(selectedBarcode.Title));
			Assert.That(item.CanEditAmount, Is.False);
		}

		[Test(Description = "Документ возврата не проходит валидацию, если количество строки не равно количеству возвращаемых штрихкодов.")]
		public void Validate_ReturnItemWithBarcodesAndDifferentAmount_ReturnValidationError()
		{
			var selectedBarcode = new Barcode { Title = "100001" };
			var issuedOperation = CreateEmployeeIssueOperation(selectedBarcode);
			var document = new Return();
			var item = new ReturnItem(document, issuedOperation, 1, new[] { selectedBarcode }) {
				Nomenclature = new Nomenclature()
			};
			item.Amount = 2;
			document.Items.Add(item);

			var errors = document.Validate(new ValidationContext(document)).ToList();

			Assert.That(errors, Has.Some.Matches<ValidationResult>(x =>
				x.ErrorMessage.Contains("количество должно быть равно количеству выбранных штрихкодов")));
		}

		[Test(Description = "Для строки возврата без штрихкодов количество можно редактировать.")]
		public void CanEditAmount_ReturnItemWithoutBarcodes_ReturnTrue()
		{
			var issuedOperation = CreateEmployeeIssueOperation();
			var document = new Return();

			var item = new ReturnItem(document, issuedOperation, 1);

			Assert.That(item.CanEditAmount, Is.True);
		}

		private static OverNormOperation CreateOverNormOperation(params Barcode[] barcodes)
		{
			var operation = new OverNormOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				WarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = new Warehouse(),
					Amount = barcodes.Length,
					StockPosition = new StockPosition(new Nomenclature(), 0, null, null, null)
				}
			};

			foreach(var barcode in barcodes) {
				var barcodeOperation = new BarcodeOperation {
					Barcode = barcode,
					OverNormOperation = operation
				};
				operation.BarcodeOperations.Add(barcodeOperation);
				barcode.BarcodeOperations.Add(barcodeOperation);
			}

			return operation;
		}

		private static EmployeeIssueOperation CreateEmployeeIssueOperation(params Barcode[] barcodes)
		{
			var operation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = barcodes.Length
			};

			foreach(var barcode in barcodes) {
				var barcodeOperation = new BarcodeOperation {
					Barcode = barcode,
					EmployeeIssueOperation = operation
				};
				operation.BarcodeOperations.Add(barcodeOperation);
				barcode.BarcodeOperations.Add(barcodeOperation);
			}

			return operation;
		}

		private static DutyNormIssueOperation CreateDutyNormIssueOperation(params Barcode[] barcodes)
		{
			var operation = new DutyNormIssueOperation {
				DutyNorm = new DutyNorm(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = barcodes.Length
			};

			foreach(var barcode in barcodes) {
				var barcodeOperation = new BarcodeOperation {
					Barcode = barcode,
					DutyNormIssueOperation = operation
				};
				operation.BarcodeOperations.Add(barcodeOperation);
				barcode.BarcodeOperations.Add(barcodeOperation);
			}

			return operation;
		}
	}
}
