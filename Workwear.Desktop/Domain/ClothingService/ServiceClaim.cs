using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Project.Domain;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;

namespace Workwear.Domain.ClothingService {
	[HistoryTrace]
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на обслуживание",
		Nominative = "заявка на обслуживание",
		Genitive = "заявки на обслуживание")]
	public class ServiceClaim : PropertyChangedBase, IDomainObject {
		#region Cвойства
		public virtual int Id { get; set; }
		
		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}
		
		private Barcode barcode;
		[Display(Name = "Штрихкод")]
		public virtual Barcode Barcode {
			get { return barcode; }
			set { SetField(ref barcode, value, () => Barcode); }
		}
		
		private bool isClosed;
		[Display(Name = "Закрыта")]
		public virtual bool IsClosed {
			get { return isClosed; }
			set { SetField(ref isClosed, value, () => IsClosed); }
		}
		
		private bool needForRepair;
		[Display(Name = "Требуется ремонт")]
		public virtual bool NeedForRepair {
			get { return needForRepair; }
			set { SetField(ref needForRepair, value, () => NeedForRepair); }
		}
		
		private string defect;
		[Display(Name = "Дефект")]
		public virtual string Defect {
			get { return defect; }
			set { SetField(ref defect, value, () => Defect); }
		}

		private uint preferredTerminalId;
		[Display(Name = "Предпочтительный постамат выдачи")]
		public virtual uint PreferredTerminalId {
			get => preferredTerminalId;
			set => SetField(ref preferredTerminalId, value);
		}
		
		private string comment;
		[Display(Name = "Коментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}
		
		private IObservableList<StateOperation> states = new ObservableList<StateOperation>();
		[Display(Name = "История состояний")]
		public virtual IObservableList<StateOperation> States {
			get { return states; }
			set { SetField(ref states, value, () => States); }
		}
		#endregion

		#region Статусы
		public virtual void ChangeState(ClaimState state, uint? terminalId = null, UserBase user = null, string comment = null) {
			var stateOperation = new StateOperation {
				Claim = this,
				OperationTime = DateTime.Now,
				State = state,
				TerminalId = terminalId,
				User = user,
				Comment = comment
			};
			States.Add(stateOperation);
		}
		#endregion
		
		#region Вычисляемые
		public virtual string Title => $"Заявка на обслуживание №{Id}";

		#endregion
	}
}
