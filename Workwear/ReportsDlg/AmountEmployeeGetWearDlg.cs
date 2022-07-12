using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;

namespace workwear.ReportsDlg
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AmountEmployeeGetWearDlg : Gtk.Bin, IParametersWidget
	{
		public string Identifier;
		public string Title { get; }

		public AmountEmployeeGetWearDlg(string Identifier, string Title)
		{
			this.Identifier = Identifier;
			this.Title = Title;
			this.Build();
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = Identifier,
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
