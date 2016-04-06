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
			var list = new List<Quarter> ();
			var quarter = new Quarter ((DateTime.Today.Month + 2) / 3, DateTime.Today.Year);
			for(int i = 0; i < 5; i++)
			{
				list.Add (quarter);
				quarter = quarter.GetNext ();
			}
			comboQuarter.ItemsList = list;
			comboQuarter.SelectedItem = list [1];
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			var quart = comboQuarter.SelectedItem as Quarter;
			string param = String.Format("quarter={0}&year={1}", quart.Number, quart.Year) ;
			ViewReportExt.Run("QuarterRequestSheet", param);
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
}

