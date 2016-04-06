using System;
using QSProjectsLib;

namespace workwear
{
	public partial class NotIssuedSheetReportDlg : Gtk.Dialog
	{
		public NotIssuedSheetReportDlg ()
		{
			this.Build ();
			ydateReport.Date = DateTime.Today;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string param = String.Format("report_date={0}&only_missing={1}", ydateReport.Date.ToString("s"), checkOnlyMissing.Active.ToString ()) ;
			ViewReportExt.Run("NotIssuedSheet", param);
		}
	}
}

