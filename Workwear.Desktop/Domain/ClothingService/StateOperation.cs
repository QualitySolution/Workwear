using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Workwear.Domain.ClothingService {
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
		#endregion
	}

	public enum ClaimState {
		[Display(Name = "Принята")]
		WaitService,
		[Display(Name = "В пути")]
		InTransit,
		[Display(Name = "В ремонте")]
		InRepair,
		[Display(Name = "В стирке")]
		InWashing,
		[Display(Name = "Ожидает выдачи")]
		AwaitIssue,
		[Display(Name = "Возвращена")]
		Returned,
	}
}
