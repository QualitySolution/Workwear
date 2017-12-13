using System;
using System.Collections.Generic;
using QSReport;

namespace workwear
{
	public partial class RequestSheetDlg : Gtk.Bin, IParametersWidget
	{
		public string Title => "Заявка на спецодежду";

		public RequestSheetDlg ()
		{
			this.Build ();
			SwitchDialog(PeriodType.Month);
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private ReportInfo GetReportInfo()
		{
			var quart = comboQuarter.SelectedItem as Quarter;
			if (quart != null)
			{
				return new ReportInfo
				{
					Identifier = "QuarterRequestSheet",
					Parameters = new Dictionary<string, object>
				{
					{ "quarter", quart.Number },
					{ "year", quart.Year },
				}
				};
			}
			var month = comboQuarter.SelectedItem as Month;
			if (month != null)
			{
				return new ReportInfo
				{
					Identifier = "MonthRequestSheet",
					Parameters = new Dictionary<string, object>
				{
					{ "month", month.Number },
					{ "year", month.Year },
				}};
			}

			var year = comboQuarter.SelectedItem as Year;
			if (year != null)
			{
				return new ReportInfo
				{
					Identifier = "YearRequestSheet",
					Parameters = new Dictionary<string, object>
					{
						{ "year", year.Number },
					}
				};
			}

			return null;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), true));
			}
		}

		private void SwitchDialog(PeriodType type)
		{
			switch (type)
			{
				case PeriodType.Year:
					var listy = new List<Year>();
					var year = new Year(DateTime.Today);
					for (int i = 0; i < 3; i++)
					{
						listy.Add(year);
						year = year.GetNext();
					}
					comboQuarter.ItemsList = listy;
					comboQuarter.SelectedItem = listy[1];
					break;

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
			Quarter,
			Year
		}

		protected void OnRadioYearToggled(object sender, EventArgs e)
		{
			if (radioYear.Active)
				SwitchDialog(PeriodType.Year);
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

	public class Year
	{
		public int Number;

		public string Title
		{
			get
			{
				return Number.ToString();
			}
		}

		public Year(int year)
		{
			Number = year;
		}

		public Year(DateTime date)
		{
			Number = date.Year;
		}

		public Year GetNext()
		{
			return new Year(Number + 1);
		}
	}

}

