using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using Workwear.Domain.Stock;
using Workwear.Domain.Supply;

namespace Workwear.Test.Domain.Supply {
	[TestFixture(TestOf = typeof(Shipment))]
	public class ShipmentTest {
		[Test(Description = "При полном поступлении заказанного ставим статус Оприходовано")]
		public void UpdateStatus_FullReceivedCase() {
			var shipment = new Shipment { Status = ShipmentStatus.Ordered };
			var item = shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);
			item.Ordered = 5;
			item.Received = 5;

			shipment.UpdateStatus();

			Assert.That(shipment.FullReceived, Is.True);
			Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Received));
		}

		[Test(Description = "При частичном поступлении статус остаётся Заказано")]
		public void UpdateStatus_PartReceivedCase() {
			var shipment = new Shipment { Status = ShipmentStatus.Ordered };
			var item = shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);
			item.Ordered = 5;
			item.Received = 3;

			shipment.UpdateStatus();

			Assert.That(shipment.FullReceived, Is.False);
			Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Ordered));
		}

		[Test(Description = "После отвязки или удаления поступления возвращаем статус Заказано, если что-то заказано")]
		public void UpdateStatus_ReceiveRemovedWithOrderedCase() {
			var shipment = new Shipment { Status = ShipmentStatus.Received, HasReceive = true, FullReceived = true };
			var item = shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);
			item.Ordered = 5;
			item.Received = 0;

			shipment.UpdateStatus();

			Assert.That(shipment.HasReceive, Is.False);
			Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Ordered));
		}

		[Test(Description = "После отвязки или удаления поступления ставим статус Новая, если ничего не заказано и не получено")]
		public void UpdateStatus_ReceiveRemovedWithoutOrderedCase() {
			var shipment = new Shipment { Status = ShipmentStatus.Received };
			var item = shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);
			item.Ordered = 0;
			item.Received = 0;

			shipment.UpdateStatus();

			Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.New));
		}

		[Test(Description = "Статус выставленный пользователем до заказа не меняем, пока нет полного поступления")]
		public void UpdateStatus_KeepUserStatusCase() {
			var shipment = new Shipment { Status = ShipmentStatus.Accepted };
			var item = shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);
			item.Ordered = 5;

			shipment.UpdateStatus();

			Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Accepted));
		}

		[Test(Description = "Заказано без заказанного количества не откатываем, такое не пропускает валидация")]
		public void UpdateStatus_KeepOrderedWithoutOrderedAmountCase() {
			var shipment = new Shipment { Status = ShipmentStatus.Ordered };
			shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);

			shipment.UpdateStatus();

			Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Ordered));
		}

		[Test(Description = "Для статуса Заказано требуем заказанное количество")]
		public void Validate_OrderedWithoutOrderedAmountCase() {
			var shipment = new Shipment {
				Status = ShipmentStatus.Ordered,
				StartPeriod = new DateTime(2026, 1, 1),
				EndPeriod = new DateTime(2026, 2, 1)
			};
			var item = shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);
			item.Ordered = 5;
			var validationContext = new ValidationContext(shipment);

			var errorsBefore = shipment.Validate(validationContext).ToList();
			item.Ordered = 0;
			var errors = shipment.Validate(validationContext).ToList();

			Assert.That(errors, Has.Count.EqualTo(errorsBefore.Count + 1));
		}

		[Test(Description = "Для статусов до Заказано заказанное количество не требуем")]
		public void Validate_AcceptedWithoutOrderedAmountCase() {
			var shipment = new Shipment { Status = ShipmentStatus.Accepted };
			shipment.AddItem(new Nomenclature { Id = 1 }, null, null, 5);

			var errors = shipment.Validate(new ValidationContext(shipment)).ToList();

			//может падать при других изменениях валидации
			Assert.That(errors, Is.Empty);
		}
	}
}
