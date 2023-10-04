using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Workwear.Domain.ClothingService {
	public class StateOperation : PropertyChangedBase, IDomainObject{
		#region Свойства
		public int Id { get; set; }
		
		private ServiceClaim claim;
		[Display(Name = "Заявка")]
		public virtual ServiceClaim Claim {
			get { return claim; }
			set { SetField(ref claim, value, () => Claim); }
		}
		
		private DateTime operationTime;
		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime {
			get { return operationTime; }
			set { SetField(ref operationTime, value, () => OperationTime); }
		}
		
		private ClaimState state;
		[Display(Name = "Состояние")]
		public virtual ClaimState State {
			get { return state; }
			set { SetField(ref state, value, () => State); }
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
