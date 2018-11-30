using System;
using QSProjectsLib;
using workwear.Domain.Operations;
using workwear.Repository.Operations;

namespace workwear.DTO
{
	public class EmployeeCardMovements
	{
		public EmployeeIssueOperation Operation { get; set; }
		public ReferencedDocument ReferencedDocument { get; set; }

		public DateTime Date => Operation.OperationTime;
		public int DocumentId => ReferencedDocument.DocId;
		public string NomenclatureName => Operation.Nomenclature.Name;
		public string UnitsName => Operation.Nomenclature.Type.Units.Name;
		public decimal? WearPercet => Operation.WearPercent;
		public decimal? Cost => Operation.IncomeOnStock?.Cost;

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

		public EmployeeCardMovements ()
		{
		}
	}

	public enum MovementType
	{
		Received,
		Returned,
		Writeoff
	};
}

