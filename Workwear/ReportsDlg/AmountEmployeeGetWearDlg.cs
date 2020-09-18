using System;
using System.Collections.Generic;
using QS.Report;
using QSProjectsLib;
using QSReport;

namespace workwear.ReportsDlg
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AmountEmployeeGetWearDlg : Gtk.Bin, IParametersWidget
	{
		public AmountEmployeeGetWearDlg()
		{
			this.Build();
		}

		public string Title => "Отчет о количестве выданного";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "AmountEmployeeGetWear",
				Parameters = new Dictionary<string, object>
				{
					{"dateStart", ydateperiodpicker.StartDate.Date },
					{"dateEnd", ydateperiodpicker.EndDate.Date}
				}
			};
		}

		protected void OnButtonPrintReportClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}

		protected void OnYdateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			buttonPrintReport.Sensitive = ydateperiodpicker.StartDate.Year > 1990;
		}
	}
}
