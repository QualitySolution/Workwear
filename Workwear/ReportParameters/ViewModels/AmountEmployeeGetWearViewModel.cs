using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.ViewModels.Extension;
using Workwear.Tools;

namespace Workwear.ReportParameters.ViewModels {
	public class AmountEmployeeGetWearViewModel : ReportParametersUowViewModelBase, IDialogDocumentation {
		public AmountEmployeeGetWearViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory)
			: base(rdlViewerViewModel, uowFactory) {
			Title = "Количество сотрудников получивших СИЗ";
			Identifier = "AmountEmployeeGetWear";
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("reports.html#amount-employees-get-wear");
		public string ButtonTooltip => DocHelper.GetReportDocTooltip(Title);
		#endregion

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"dateStart", StartDate },
			{"dateEnd", EndDate },
		};

		private DateTime? startDate = DateTime.Today.AddMonths(-1);
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}

		private DateTime? endDate = DateTime.Today;
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}

		public bool SensitiveLoad => StartDate != null && EndDate != null;
	}
}
