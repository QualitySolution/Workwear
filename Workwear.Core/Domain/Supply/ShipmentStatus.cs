using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Supply {
	public enum ShipmentStatus {
		[Display(Name="Заказано")]
		ordered,
		[Display(Name = "В пути")]
		on_the_way,
		[Display(Name="Ожидает оплаты")]
		awaiting_payment,
		[Display(Name="Отменено")]
		cancelled,
		[Display(Name = "Получено")]
		received
	
	}
}
