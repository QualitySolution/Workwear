using System;
using QSProjectsLib;

namespace workwear.DTO
{
	public class EmployeeCardMovements
	{
		public DateTime Date { get; set;}
		public MovementType MovementType { get; set;}
		public int DocumentId { get; set;}
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
		public decimal? Life { get; set;}
		public decimal? Cost { get; set;}

		public int AmountReceived { get; set;}
		public int AmountReturned { get; set;}

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
				switch (MovementType) {
				case MovementType.Received:
					return String.Format ("Выдача №{0}", DocumentId);
				case MovementType.Returned:
					return String.Format ("Возврат №{0}", DocumentId);
				case MovementType.Writeoff:
					return String.Format ("Списание №{0}", DocumentId);
				default:
					return String.Empty;
				}
			}}

		public string LifeText {get { 
				return Life.HasValue ? Life.Value.ToString ("P0") : String.Empty;
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

