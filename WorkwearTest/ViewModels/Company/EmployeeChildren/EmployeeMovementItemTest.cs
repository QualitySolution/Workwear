using NUnit.Framework;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace WorkwearTest.ViewModels.Company.EmployeeChildren {
	[TestFixture(TestOf = typeof(EmployeeMovementItem))]
	public class EmployeeMovementItemTest {
		[Test(Description = "Проверяем формирование списка штрихкодов, один")]
		public void BarcodesString_OneBarcode() {
			var item = new EmployeeMovementItem {
				Operation = new EmployeeIssueOperation()
			};
			item.Operation.BarcodeOperations.Add(
				new BarcodeOperation {
					Barcode = new Barcode {
						Title = "2040000066574"
					}
				});
			Assert.That(item.BarcodesString, Is.EqualTo("2040000066574"));
		}
		
		[Test(Description = "Проверяем формирование списка штрихкодов, два")]
		public void BarcodesString_TwoBarcode() {
			var item = new EmployeeMovementItem {
				Operation = new EmployeeIssueOperation()
			};
			item.Operation.BarcodeOperations.Add(
				new BarcodeOperation {
					Barcode = new Barcode {
						Title = "2040000066574"
					}
				});
			item.Operation.BarcodeOperations.Add(
				new BarcodeOperation {
					Barcode = new Barcode {
						Title = "2040000066581"
					}
				});
			Assert.That(item.BarcodesString, Is.EqualTo("2040000066574\n2040000066581"));
		}
		
		[Test(Description = "Проверяем формирование списка штрихкодов, три")]
		public void BarcodesString_ThreeBarcode() {
			var item = new EmployeeMovementItem {
				Operation = new EmployeeIssueOperation()
			};
			item.Operation.BarcodeOperations.Add(
				new BarcodeOperation {
					Barcode = new Barcode {
						Title = "2040000066574"
					}
				});
			item.Operation.BarcodeOperations.Add(
				new BarcodeOperation {
					Barcode = new Barcode {
						Title = "2040000066581"
					}
				});
			item.Operation.BarcodeOperations.Add(
				new BarcodeOperation {
					Barcode = new Barcode {
						Title = "2040000073282"
					}
				});
			Assert.That(item.BarcodesString, Is.EqualTo("2040000066574\n2040000066581\n2040000073282"));
		}
	}
}
