using System;
using System.Collections.Generic;
using QSProjectsLib;
using QSReport;

namespace workwear
{
	public partial class NotIssuedSheetReportDlg : Gtk.Bin, IParametersWidget
	{
		public NotIssuedSheetReportDlg ()
		{
			this.Build ();
			ydateReport.Date = DateTime.Today;
		}

		string IParametersWidget.Title => "Справка по невыданному";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), true));
			}
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "NotIssuedSheet",
				Parameters = new Dictionary<string, object>
				{
					{ "report_date", ydateReport.Date },
					{ "only_missing", checkOnlyMissing.Active },
				}
			};
		}

		protected void OnYdateReportDateChanged(object sender, EventArgs e)
		{
			buttonRun.Sensitive = !ydateReport.IsEmpty;
		}
	}
}