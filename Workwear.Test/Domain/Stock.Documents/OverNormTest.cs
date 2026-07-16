using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Test.Domain.Stock.Documents {
	[TestFixture(TestOf = typeof(OverNorm))]
	public class OverNormTest {
		[Test(Description = "Документ выдачи вне нормы не должен содержать один штрихкод в нескольких строках.")]
		public void Validate_DuplicateBarcode_ReturnValidationError()
		{
			var barcode = new Barcode { Title = "100001" };
			var nomenclature = new Nomenclature();
			var document = new OverNorm {
				Warehouse = new Warehouse()
			};
			document.AddItem(CreateOperation(barcode, nomenclature));
			document.AddItem(CreateOperation(barcode, nomenclature));

			var errors = document.Validate(new ValidationContext(document)).ToList();

			Assert.That(errors.Select(x => x.ErrorMessage), Has.Some.Contains("несколько раз добавлены"));
			Assert.That(errors.Select(x => x.ErrorMessage), Has.Some.Contains(barcode.Title));
		}

		private static OverNormOperation CreateOperation(Barcode barcode, Nomenclature nomenclature)
		{
			var operation = new OverNormOperation {
				Employee = new EmployeeCard(),
				WarehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1
				}
			};
			var barcodeOperation = new BarcodeOperation {
				Barcode = barcode,
				OverNormOperation = operation
			};
			operation.BarcodeOperations.Add(barcodeOperation);
			barcode.BarcodeOperations.Add(barcodeOperation);
			return operation;
		}
	}
}
