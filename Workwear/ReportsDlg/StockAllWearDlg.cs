using System;
using System.Collections.Generic;
using QS.Report;
using QSProjectsLib;
using QSReport;
using workwear.Tools.Features;

namespace workwear.ReportsDlg
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class StockAllWearDlg : Gtk.Bin, IParametersWidget
	{
		private FeaturesService featureService;

		public StockAllWearDlg()
		{
			this.Build();
			ComboWorks.ComboFillReference(comboObject, "warehouse", ComboWorks.ListMode.OnlyItems, true, "name");
			featureService = new FeaturesService();
			DisableFeatures();
			comboObject.Active = 0;
		}

		private void DisableFeatures()
		{
			if(!featureService.Available(WorkwearFeature.Warehouses)) {
				table1.Visible = false;
				
			}
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
