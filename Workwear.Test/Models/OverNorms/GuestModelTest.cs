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
	[TestFixture(TestOf = typeof(GuestModel))]
	public class GuestModelTest {
		[Test(Description = "При добавлении строки без штрихкодов в гостевую выдачу все метки ранее заполненной строки сохраняются.")]
		public void UpdateOperation_WithoutBarcodes_KeepsBarcodesOnOtherItem()
		{
			var model = new GuestModel(Substitute.For<IUnitOfWork>());
			var employee = new EmployeeCard();
			var nomenclature = new Nomenclature { UseBarcode = true };
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

			model.UseBarcodes = true;
			model.UpdateOperation(item, param);

			Assert.That(document.Items, Has.Count.EqualTo(1));
			Assert.That(document.Items.Single(), Is.SameAs(item));

			var unmarkedItem = document.AddItem(new OverNormOperation { Employee = employee });
			var unmarkedNomenclature = new Nomenclature { UseBarcode = false };
			model.UseBarcodes = false;
			model.UpdateOperation(unmarkedItem, new OverNormParam(employee, unmarkedNomenclature, 3));

			Assert.That(unmarkedItem.Amount, Is.EqualTo(3));
			Assert.That(unmarkedItem.Barcodes, Is.Empty);
			Assert.That(operation.WarehouseOperation.Amount, Is.EqualTo(2));
			Assert.That(operation.BarcodeOperations, Has.Count.EqualTo(2));
			Assert.That(operation.BarcodeOperations.Select(x => x.Barcode), Is.EquivalentTo(new[] { barcode1, barcode2 }));
			Assert.That(operation.BarcodeOperations.Select(x => x.WarehouseOperation), Is.All.SameAs(operation.WarehouseOperation));
			Assert.That(barcode1.BarcodeOperations, Has.Count.EqualTo(1));
			Assert.That(barcode2.BarcodeOperations, Has.Count.EqualTo(1));
		}
	}
}
