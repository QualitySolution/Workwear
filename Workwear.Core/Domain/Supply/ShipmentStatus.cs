using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Supply {
	public enum ShipmentStatus {
		[Display(Name="Черновик")]
		New,
		[Display(Name = "Передано в закупку")]
		Present,
		[Display(Name="Принято в работу")]
		Accepted,
		[Display(Name="Заказано")]
		Ordered,
		[Display(Name = "Оприходовано")]
		Received
	}
}
