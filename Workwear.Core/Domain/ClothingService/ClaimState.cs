using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.ClothingService {
	public enum ClaimState {
		[Display(Name = "Принята")]
		WaitService,
		[Display(Name = "Принят терминалом")]
		InReceiptTerminal,
		[Display(Name = "В пути")]
		InTransit,
		[Display(Name = "В ремонте")]
		InRepair,
		[Display(Name = "В стирке")]
		InWashing,
		[Display(Name = "Ожидает выдачи")]
		AwaitIssue,
		[Display(Name = "В терминале выдачи")]
		InDispenseTerminal,
		[Display(Name = "Возвращена")]
		Returned,
	}
}
