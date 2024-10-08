using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Utilities;
using Workwear.Domain.Operations;
using Workwear.Models.Operations;
using Workwear.Domain.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren
{
	public class EmployeeMovementItem : PropertyChangedBase
	{
		public EmployeeIssueOperation Operation { get; set; }
		public OperationToDocumentReference EmployeeIssueReference { get; set; }

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

		public IEnumerable<Barcode> Barcodes => Operation.BarcodeOperations.Select(bo => bo.Barcode);

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

		public string  DocumentTitle {
			get {
				if(EmployeeIssueReference?.DocumentType != null)
					return $"{EmployeeIssueReference.DocumentType.GetEnumTitle()} №{EmployeeIssueReference.DocumentId}";
				if(Operation.ManualOperation)
					return "Ручная операция";
				return String.Empty;
			}
		}

		public string BarcodesString {
			get {
				if(!Barcodes.Any())
					return String.Empty;
				return Barcodes.DefaultIfEmpty().Select(bc => bc.Title).Aggregate((a,b) => a + "\n" + b);
			}
		}

		public string ProtectionTools {
			get {
				return Operation?.ProtectionTools?.Name ?? String.Empty;
			}
		}
	}
}
