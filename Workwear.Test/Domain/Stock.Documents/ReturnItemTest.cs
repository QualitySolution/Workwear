using System.Linq;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
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
	}
}
