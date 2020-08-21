using System;
using QS.DomainModel.Entity;
using QSProjectsLib;
using workwear.Domain.Operations;
using workwear.Repository.Operations;

namespace workwear.DTO
{
	public class EmployeeCardMovements : PropertyChangedBase
	{
		public EmployeeIssueOperation Operation { get; set; }
		public ReferencedDocument ReferencedDocument { get; set; }

		public DateTime Date => Operation.OperationTime;
		public int DocumentId => ReferencedDocument.DocId;
		public string NomenclatureName => Operation.Nomenclature.Name;
		public string UnitsName => Operation.Nomenclature.Type.Units.Name;
		public decimal? WearPercet => Operation.WearPercent;
		public decimal? Cost => Operation.WarehouseOperation?.Cost;

		public int AmountReceived => Operation.Issued;
		public int AmountReturned => Operation.Returned;

		public string AmountReceivedText {get{ 
				return AmountReceived > 0 ? String.Format ("{0} {1}", AmountReceived, UnitsName) : String.Empty;
			}}

		public string AmountReturnedText {get{ 
				return AmountReturned > 0 ? String.Format ("{0} {1}", AmountReturned, UnitsName) : String.Empty;
			}}

		public string CostText {get { 
				return Cost.HasValue ? CurrencyWorks.GetShortCurrencyString (Cost.Value) : String.Empty;
			}}

		public string DocumentName {get{ 
				switch (ReferencedDocument?.DocType) {
				case EmployeeIssueOpReferenceDoc.ReceivedFromStock:
					return String.Format ("Выдача №{0}", DocumentId);
				case EmployeeIssueOpReferenceDoc.RetutnedToStock:
					return String.Format ("Возврат №{0}", DocumentId);
				case EmployeeIssueOpReferenceDoc.WriteOff:
					return String.Format ("Списание №{0}", DocumentId);
				default:
					return String.Empty;
				}
			}}

		public string WearPercentText {get { 
				return WearPercet.HasValue ? WearPercet.Value.ToString ("P0") : String.Empty;
			}}

		[PropertyChangedAlso(nameof(AutoWriteOffDateTextColored))]
		public bool UseAutoWriteOff {
			get {
				return Operation.UseAutoWriteoff;
			}
			set {
				Operation.UseAutoWriteoff = value;
				OnPropertyChanged();
			}
		}

		public string AutoWriteOffDateTextColored {
			get {
				if(Operation.AutoWriteoffDate == null)
					return null;
				string color;
				if(Operation.AutoWriteoffDate.Value.Date == DateTime.Today)
					color = "blue";
				else if(Operation.AutoWriteoffDate.Value.Date < DateTime.Today)
					color = "green";
				else
					color = "purple";
				return String.Format("<span foreground=\"{1}\">{0}</span>", Operation.AutoWriteoffDate?.ToShortDateString(), color);
			}
		}

		public EmployeeCardMovements ()
		{
		}
	}
}

