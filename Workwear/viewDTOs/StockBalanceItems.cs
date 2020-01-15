using System;
using QSProjectsLib;

namespace workwear.DTO
{
	public class StockBalanceItems
	{
		public int NomenclatureId { get; set;}
		public string NomenclatureName { get; set;}
		public string UnitsName { get; set;}
		public string Size { get; set;}
		public string Growth { get; set;}
		public decimal AvgCost { get; set;}
		public decimal AvgLife { get; set;}

		public int Amount { get; set;}

		public string AmountText {get{ return String.Format ("{0} {1}", Amount, UnitsName);
			}}

		public string AvgCostText {get { 
				return AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
			}}

		public string AvgLifeText {get { 
				return AvgLife.ToString ("P");
			}}

		public StockBalanceItems ()
		{
			

		}
	}
}

