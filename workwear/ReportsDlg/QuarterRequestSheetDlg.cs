using System;
using QSProjectsLib;
using System.Collections.Generic;

namespace workwear
{
	public partial class QuarterRequestSheetDlg : Gtk.Dialog
	{
		public QuarterRequestSheetDlg ()
		{
			this.Build ();
			SwitchDialog(PeriodType.Month);
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			var quart = comboQuarter.SelectedItem as Quarter;
			if(quart != null)
			{
				string param = String.Format("quarter={0}&year={1}", quart.Number, quart.Year) ;
				ViewReportExt.Run("QuarterRequestSheet", param);
				return;
			}
			var month = comboQuarter.SelectedItem as Month;
			if(month != null)
			{
				string param = String.Format("month={0}&year={1}", month.Number, month.Year) ;
				ViewReportExt.Run("MonthRequestSheet", param);
				return;
			}
		}

		private void SwitchDialog(PeriodType type)
		{
			switch (type)
			{
				case PeriodType.Quarter:
					var list = new List<Quarter> ();
					var quarter = new Quarter ((DateTime.Today.Month + 2) / 3, DateTime.Today.Year);
					for(int i = 0; i < 5; i++)
					{
						list.Add (quarter);
						quarter = quarter.GetNext ();
					}
					comboQuarter.ItemsList = list;
					comboQuarter.SelectedItem = list [1];
					break;

				case PeriodType.Month:
					var listM = new List<Month> ();
					var month = new Month (DateTime.Today.AddMonths(-1));
					for(int i = 0; i < 14; i++)
					{
						listM.Add (month);
						month = month.GetNext ();
					}
					comboQuarter.ItemsList = listM;
					comboQuarter.SelectedItem = listM [2];
					break;
			}
		}

		protected void OnRadioMonthToggled(object sender, EventArgs e)
		{
			if (radioMonth.Active)
				SwitchDialog(PeriodType.Month);
		}

		protected void OnRadioQuarterToggled(object sender, EventArgs e)
		{
			if (radioQuarter.Active)
				SwitchDialog(PeriodType.Quarter);
		}

		enum PeriodType{
			Month,
			Quarter
		}
	}

	public class Quarter{
		public int Number;
		public int Year;

		public string Title{
			get{ return String.Format ("{0} квартал {1}", Number, Year);
			}
		}

		public Quarter(int num, int year)
		{
			Number = num;
			Year = year;
		}

		public Quarter GetNext()
		{
			int newNum = Number + 1;
			return newNum == 5 ? new Quarter (1, Year + 1) : new Quarter (newNum, Year);
		}
	}

	public class Month{
		public int Number;
		public int Year;

		public string Title{
			get{ return String.Format ("{0:MMMM yyyy}", new DateTime(Year, Number, 1));
			}
		}

		public Month(int num, int year)
		{
			Number = num;
			Year = year;
		}

		public Month(DateTime date)
		{
			Number = date.Month;
			Year = date.Year;
		}

		public Month GetNext()
		{
			int newNum = Number + 1;
			return newNum == 13 ? new Month (1, Year + 1) : new Month (newNum, Year);
		}
	}

}

