using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Models;

namespace Workwear.Test.Models.OverNorms {
	[TestFixture(TestOf = typeof(SimpleModel))]
	public class SimpleModelTest {
		[Test(Description = "При изменении штрихкодов в сохраненной разовой выдаче сохраняем операции для оставшихся штрихкодов.")]
		public void UpdateOperation_ChangeBarcodes_PreserveBarcodeOperationsForSameBarcodes()
		{
			var model = new SimpleModel(Substitute.For<IUnitOfWork>()) {
				UseBarcodes = true
			};
			var employee = new EmployeeCard();
			var nomenclature = new Nomenclature();
			var removedBarcode = new Barcode { Nomenclature = nomenclature };
			var savedBarcode = new Barcode { Nomenclature = nomenclature };
			var newBarcode = new Barcode { Nomenclature = nomenclature };
			var operation = new OverNormOperation {
				Employee = employee,
				WarehouseOperation = new WarehouseOperation {
					Amount = 2,
					Nomenclature = nomenclature
				}
			};
			var barcodeOperation1 = new BarcodeOperation {
				Id = 10,
				Barcode = removedBarcode,
				OverNormOperation = operation
			};
			var barcodeOperation2 = new BarcodeOperation {
				Id = 11,
				Barcode = savedBarcode,
				OverNormOperation = operation
			};
			operation.BarcodeOperations.Add(barcodeOperation1);
			operation.BarcodeOperations.Add(barcodeOperation2);
			removedBarcode.BarcodeOperations.Add(barcodeOperation1);
			savedBarcode.BarcodeOperations.Add(barcodeOperation2);
			var item = new OverNormItem(new OverNorm(), operation);
			var param = new OverNormParam(
				employee,
				nomenclature,
				2,
				barcodes: new List<Barcode> { savedBarcode, newBarcode });

			model.UpdateOperation(item, param);

			Assert.That(operation.BarcodeOperations.Count, Is.EqualTo(2));
			Assert.That(operation.BarcodeOperations, Does.Not.Contain(barcodeOperation1));
			Assert.That(operation.BarcodeOperations, Does.Contain(barcodeOperation2));
			Assert.That(barcodeOperation2.Id, Is.EqualTo(11));
			Assert.That(barcodeOperation2.Barcode, Is.SameAs(savedBarcode));
			Assert.That(removedBarcode.BarcodeOperations, Does.Not.Contain(barcodeOperation1));
			Assert.That(newBarcode.BarcodeOperations, Has.Count.EqualTo(1));
			Assert.That(newBarcode.BarcodeOperations[0].OverNormOperation, Is.SameAs(operation));
		}

		[Test(Description = "При заполнении пустой строки разовой выдачи несколькими штрихкодами добавляем все выбранные метки.")]
		public void UpdateOperation_EmptyItemWithSeveralBarcodes_AddAllBarcodeOperations()
		{
			var model = new SimpleModel(Substitute.For<IUnitOfWork>()) {
				UseBarcodes = true
			};
			var employee = new EmployeeCard();
			var nomenclature = new Nomenclature();
			var barcode1 = new Barcode { Nomenclature = nomenclature };
			var barcode2 = new Barcode { Nomenclature = nomenclature };
			var document = new OverNorm {
				Warehouse = new Warehouse()
			};
			var operation = new OverNormOperation {
				Employee = employee
			};
			document.AddItem(operation);
			var item = document.Items.Single();
			var param = new OverNormParam(
				employee,
				nomenclature,
				2,
				barcodes: new List<Barcode> { barcode1, barcode2 });

			model.UpdateOperation(item, param);

			Assert.That(document.Items, Has.Count.EqualTo(1));
			Assert.That(document.Items.Single(), Is.SameAs(item));
			Assert.That(operation.WarehouseOperation.Amount, Is.EqualTo(2));
			Assert.That(operation.BarcodeOperations, Has.Count.EqualTo(2));
			Assert.That(operation.BarcodeOperations.Select(x => x.Barcode), Is.EquivalentTo(new[] { barcode1, barcode2 }));
			Assert.That(barcode1.BarcodeOperations, Has.Count.EqualTo(1));
			Assert.That(barcode2.BarcodeOperations, Has.Count.EqualTo(1));
		}
	}
}
