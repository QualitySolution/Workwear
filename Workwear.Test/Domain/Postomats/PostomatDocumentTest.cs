using System.Linq;
using NUnit.Framework;
using QS.Cloud.Postomat.Manage;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Postomats;
using Workwear.Domain.Stock;
using CellLocation = Workwear.Domain.Postomats.CellLocation;

namespace Workwear.Test.Domain.Postomats {
	[TestFixture(TestOf = typeof(PostomatDocument))]
	public class PostomatDocumentTest {
		[Test(Description = "Повторное добавление той же заявки не создаёт дубль строки, а у заявки остаётся статус доставки в постомат.")]
		public void AddItem_SameClaimTwice_AddsSingleItem() {
			var document = new PostomatDocument { TerminalId = 1, Postomat = new PostomatInfo { Id = 1 } };
			var claim = new ServiceClaim { Id = 10, Barcode = new Barcode { Nomenclature = new Nomenclature() } };

			document.AddItem(claim, new CellLocation(null, 1, 1, 1), null);
			document.AddItem(claim, new CellLocation(null, 1, 1, 2), null);

			Assert.That(document.Items, Has.Count.EqualTo(1));
			Assert.That(claim.States.Last().State, Is.EqualTo(ClaimState.DeliveryToDispenseTerminal));
		}
	}
}
