using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Supply {
	public enum ShipmentStatus {
		[Display(Name="Заказано")]
		Ordered,
		[Display(Name = "В пути")]
		OnTheWay,
		[Display(Name="Ожидает оплаты")]
		AwaitPayment,
		[Display(Name="Отменено")]
		Cancelled,
		[Display(Name = "Получено")]
		Received
	
	}
}
