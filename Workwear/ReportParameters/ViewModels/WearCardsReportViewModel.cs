using QS.Report.ViewModels;
using QS.ViewModels.Extension;
using System.Collections.Generic;
using Workwear.Tools;

namespace Workwear.ReportParameters.ViewModels {
	public class WearCardsReportViewModel: ReportParametersViewModelBase, IDialogDocumentation {
		public WearCardsReportViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel) {
			Title = "Список сотрудников";
			Identifier = "WearCardsReportFlat";
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("reports.html#report-employees-list");
		public string ButtonTooltip => DocHelper.GetReportDocTooltip(Title);
		#endregion
		
		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{"only_without_norms", OnlyWithoutNorms},
			{"only_working", OnlyWorking},
		};

		private bool onlyWithoutNorms;
		public virtual bool OnlyWithoutNorms {
			get => onlyWithoutNorms;
			set=>SetField(ref onlyWithoutNorms, value);
		}

		private bool onlyWorking;

		public virtual bool OnlyWorking {
			get => onlyWorking;
			set=>SetField(ref onlyWorking, value);
		}
		
	}
}
