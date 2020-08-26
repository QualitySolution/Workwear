using System;
using System.Collections.Generic;
using QS.Report;
using QSProjectsLib;
using QSReport;

namespace workwear.ReportsDlg
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class StockAllWearDlg : Gtk.Bin, IParametersWidget
	{
		public StockAllWearDlg()
		{
			this.Build();
			ComboWorks.ComboFillReference(comboObject, "warehouse", ComboWorks.ListMode.OnlyItems, true, "name");
			comboObject.Active = 0;
		}

		public string Title => "Складская ведомость";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private ReportInfo GetReportInfo()
		{
			int r = ComboWorks.GetActiveId(comboObject);

			return new ReportInfo {
				Title = "Складская ведомость",
				Identifier = "StockAllWear",
				Parameters = new Dictionary<string, object>
				{
					{ "id", ComboWorks.GetActiveId(comboObject) },
				}
			};
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), true));
			}
		}

		protected void OnComboObjectChanged(object sender, EventArgs e)
		{
			buttonRun.Sensitive = ComboWorks.GetActiveId(comboObject) > 0;
		}
	}
}
