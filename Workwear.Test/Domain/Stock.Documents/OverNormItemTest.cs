using System.Collections.Generic;
using NUnit.Framework;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Test.Domain.Stock.Documents {
	[TestFixture(TestOf = typeof(OverNormItem))]
	public class OverNormItemTest {
		[Test(Description = "При изменении операций штрихкодов строка уведомляет об изменении списка штрихкодов.")]
		public void BarcodeOperations_CollectionChanged_RaiseBarcodesPropertyChanged()
		{
			var operation = new OverNormOperation();
			var item = new OverNormItem(new OverNorm(), operation);
			var changedProperties = new List<string>();
			item.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

			operation.BarcodeOperations.Add(new BarcodeOperation {
				Barcode = new Barcode(),
				OverNormOperation = operation
			});

			Assert.That(changedProperties, Does.Contain(nameof(OverNormItem.Barcodes)));
		}
	}
}
