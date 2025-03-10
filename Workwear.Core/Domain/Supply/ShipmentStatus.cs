using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Supply {
	public enum ShipmentStatus {
		[Display(Name="Заказано")]
		Ordered,
		[Display(Name = "В пути")]
		On_the_way,
		[Display(Name="Ожидает оплаты")]
		Awaiting_payment,
		[Display(Name="Отменено")]
		Cancelled,
		[Display(Name = "Получено")]
		Received
	
	}
}
