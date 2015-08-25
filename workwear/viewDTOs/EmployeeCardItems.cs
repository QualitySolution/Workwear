using System;
using QSProjectsLib;

namespace workwear
{
	public class EmployeeCardItems
	{
		public int ItemTypeId { get; set;}
		public string ItemTypeName { get; set;}
		public string UnitsName { get; set;}
		public decimal AvgCost { get; set;}

		public int Amount { get; set;}

		public string AmountText {get{ return String.Format ("{0} {1}", Amount, UnitsName);
			}}

		public string AvgCostText {get { return CurrencyWorks.GetShortCurrencyString (AvgCost);
			}}

		public EmployeeCardItems ()
		{
			

		}
	}
}

