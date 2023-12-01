using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Project.Domain;

namespace Workwear.Domain.ClothingService {
	[HistoryTrace]
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции над заявкой на обслуживание",
		Nominative = "операция над заявкой на обслуживание")]
	public class StateOperation : PropertyChangedBase, IDomainObject{
		#region Свойства
		public virtual int Id { get; set; }
		
		private ServiceClaim claim;
		[Display(Name = "Заявка")]
		public virtual ServiceClaim Claim {
			get => claim;
			set => SetField(ref claim, value);
		}
		
		private DateTime operationTime;
		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime {
			get => operationTime;
			set => SetField(ref operationTime, value);
		}
		
		private ClaimState state;
		[Display(Name = "Состояние")]
		public virtual ClaimState State {
			get => state;
			set => SetField(ref state, value);
		}

		private UserBase user;
		[Display(Name = "Пользователь")]
		public virtual UserBase User {
			get => user;
			set => SetField(ref user, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		#endregion
		#region Расчетные
		public virtual string Title => $"Операция {State.GetEnumTitle()} заявки №{Claim.Id} от {OperationTime:d}";
		#endregion
	}

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
