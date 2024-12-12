using QS.Report.ViewModels;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.Report;

namespace Workwear.ReportParameters.ViewModels {
	public class WearCardsReportViewModel: ReportParametersViewModelBase {
		public WearCardsReportViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel) {
			Title = "Список сотрудников";
			Identifier = "WearCardsReportFlat";
		}

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
