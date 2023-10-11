using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Stock;

namespace Workwear.Domain.ClothingService {
	[HistoryTrace]
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на обслуживание",
		Nominative = "заявка на обслуживание")]
	public class ServiceClaim : PropertyChangedBase, IDomainObject {
		#region Cвойства
		public virtual int Id { get; set; }
		
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
		
		private IObservableList<StateOperation> states = new ObservableList<StateOperation>();
		[Display(Name = "История состояний")]
		public virtual IObservableList<StateOperation> States {
			get { return states; }
			set { SetField(ref states, value, () => States); }
		}
		#endregion
	}
}
