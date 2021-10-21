using System;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QSProjectsLib;
using workwear.Domain.Operations;
using workwear.Repository.Operations;

namespace workwear.DTO
{
	public class EmployeeCardMovements : PropertyChangedBase
	{
		public EmployeeIssueOperation Operation { get; set; }
		public EmployeeIssueReference EmployeeIssueReference { get; set; }

		public DateTime Date => Operation.OperationTime;
		public string NomenclatureName => Operation.Nomenclature?.Name ?? Operation.ProtectionTools.Name;
		public string UnitsName => Operation.Nomenclature?.Type.Units.Name ?? Operation.ProtectionTools.Type.Units.Name;
		public decimal? WearPercet => Operation.WearPercent;
		public decimal? Cost => Operation.WarehouseOperation?.Cost;

		public int AmountReceived => Operation.Issued;
		public int AmountReturned => Operation.Returned;

		public string AmountReceivedText => AmountReceived > 0 ? String.Format("{0} {1}", AmountReceived, UnitsName) : String.Empty;

		public string AmountReturnedText => AmountReturned > 0 ? String.Format("{0} {1}", AmountReturned, UnitsName) : String.Empty;

		public string CostText => Cost.HasValue ? CurrencyWorks.GetShortCurrencyString(Cost.Value) : String.Empty;

		public string WearPercentText => WearPercet.HasValue ? WearPercet.Value.ToString("P0") : String.Empty;

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

		public string SingText => IsSigned ? Operation.SignCardKey + " " + Operation.SignTimestamp.Value.ToString("dd.MM.yyyy HH:mm:ss") : null;

		public bool IsSigned => !String.IsNullOrEmpty(Operation.SignCardKey);

		public string DocumentTitle {
			get {
				if(EmployeeIssueReference?.DocumentType != null)
					return $"{EmployeeIssueReference.DocumentType.GetEnumTitle()} №{EmployeeIssueReference.DocumentId}";
				if(Operation.OverrideBefore)
					return "Ручная операция";
				return String.Empty;
			}
		}
	}
}
