using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.ClothingService {
	public enum ClaimState {
		[Display(Name = "Принята")]
		WaitService,
		[Display(Name = "Принят терминалом")]
		InReceiptTerminal,
		[Display(Name = "В пути")]
		InTransit,
		[Display(Name = "Доставка в прачечную")]
		DeliveryToLaundry,
		[Display(Name = "В ремонте")]
		InRepair,
		[Display(Name = "В стирке")]
		InWashing,
		[Display(Name = "В химчистке")]
		InDryCleaning,
		[Display(Name = "Ожидает выдачи")]
		AwaitIssue,
		[Display(Name = "Доставка в постамат выдачи")]
		DeliveryToDispenseTerminal,
		[Display(Name = "В постамате выдачи")]
		InDispenseTerminal,
		[Display(Name = "Возвращена")]
		Returned,
	}
}
