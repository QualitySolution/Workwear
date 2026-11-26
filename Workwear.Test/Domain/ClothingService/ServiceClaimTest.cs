using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Test.Domain.ClothingService {
	[TestFixture(TestOf = typeof(ServiceClaim))]
	public class ServiceClaimTest {
		[Test(Description = "Повторный вызов Update (например, из-за повторного Save() документа возврата) не должен задваивать статус \"Возвращена\" в истории уже закрытой заявки.")]
		public void Update_CalledTwiceForAlreadyClosedClaim_DoesNotDuplicateReturnedState() {
			var claim = new ServiceClaim { Barcode = new Barcode(), IsClosed = false };
			var document = new Return();
			var issuedOperation = new EmployeeIssueOperation { Nomenclature = new Nomenclature(), Issued = 1 };
			var item = new ReturnItem(document, issuedOperation, 1);
			var uow = Substitute.For<IUnitOfWork>();

			claim.Update(uow, item);
			claim.Update(uow, item);

			Assert.That(claim.IsClosed, Is.True);
			Assert.That(claim.States.Count(x => x.State == ClaimState.Returned), Is.EqualTo(1));
		}

		[Test(Description = "Повторный ChangeState с тем же статусом и тем же комментарием не создаёт новую запись в истории.")]
		public void ChangeState_SameStateAndSameComment_DoesNotAddNewStateOperation() {
			var claim = new ServiceClaim { Barcode = new Barcode() };

			var first = claim.ChangeState(ClaimState.InWashing, comment: "В машине №1");
			var second = claim.ChangeState(ClaimState.InWashing, comment: "В машине №1");

			Assert.That(first, Is.Not.Null);
			Assert.That(second, Is.Null);
			Assert.That(claim.States, Has.Count.EqualTo(1));
		}

		[Test(Description = "Повторный ChangeState с тем же статусом, но другим комментарием, добавляет новую запись (изменился комментарий).")]
		public void ChangeState_SameStateDifferentComment_AddsNewStateOperation() {
			var claim = new ServiceClaim { Barcode = new Barcode() };

			claim.ChangeState(ClaimState.InWashing, comment: "В машине №1");
			var second = claim.ChangeState(ClaimState.InWashing, comment: "В машине №2");

			Assert.That(second, Is.Not.Null);
			Assert.That(claim.States, Has.Count.EqualTo(2));
		}

		[Test(Description = "ChangeState с другим статусом всегда добавляет новую запись в историю.")]
		public void ChangeState_DifferentState_AddsNewStateOperation() {
			var claim = new ServiceClaim { Barcode = new Barcode() };

			claim.ChangeState(ClaimState.InTransit);
			var second = claim.ChangeState(ClaimState.InWashing);

			Assert.That(second, Is.Not.Null);
			Assert.That(claim.States, Has.Count.EqualTo(2));
		}
	}
}
