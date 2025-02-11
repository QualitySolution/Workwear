using System;
using System.Collections.Generic;
using Autofac;
using QS.Report;
using QSProjectsLib;
using QSReport;
using Workwear.Tools.Features;

namespace workwear
{
	public partial class WearStatement : Gtk.Bin, IParametersWidget
	{
		ILifetimeScope AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
		private FeaturesService featuresService;
		public WearStatement()
		{
			this.Build();
			ComboWorks.ComboFillReference(comboObject, "subdivisions", ComboWorks.ListMode.OnlyItems);
			comboObject.Active = 0;
			featuresService=AutofacScope.Resolve<FeaturesService>();
		}

		public string Title => "Сводная ведомость";

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public bool PrintPromo => featuresService.Available(WorkwearFeature.PrintPromo);

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "WearStatement",
				Parameters = new Dictionary<string, object>
				{
					{ "id", ComboWorks.GetActiveId(comboObject) },
					{"printPromo", PrintPromo},
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

