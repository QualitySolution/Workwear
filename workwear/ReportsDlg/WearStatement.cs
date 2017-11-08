using System;
using System.Collections.Generic;
using QSProjectsLib;
using QSReport;

namespace workwear
{
	public partial class WearStatement : Gtk.Bin, IParametersWidget
	{
		public WearStatement()
		{
			this.Build();
			ComboWorks.ComboFillReference(comboObject, "objects", ComboWorks.ListMode.OnlyItems);
			comboObject.Active = 0;
		}

		public string Title => "Сводная ведомость";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "WearStatement",
				Parameters = new Dictionary<string, object>
				{
					{ "id", ComboWorks.GetActiveId(comboObject) },
				}
			};
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), true));
			}
		}

		protected void OnComboObjectChanged(object sender, EventArgs e)
		{
			buttonRun.Sensitive = ComboWorks.GetActiveId(comboObject) > 0;
		}
	}
}

