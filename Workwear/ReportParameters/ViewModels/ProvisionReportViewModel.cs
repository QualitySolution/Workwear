using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class ProvisionReportViewModel : ReportParametersViewModelBase {
		
		public ProvisionReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory)
			: base(rdlViewerViewModel) {
			
			Title = $"Отчёт по обеспечености сотрудников на {reportDate?.ToString("dd MMMM yyyy")}";
			Identifier = "ProvisionReport";

			UoW = uowFactory.CreateWithoutRoot();
			reportDate = DateTime.Today;
			
			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(uowFactory,UoW);
			ChoiceSubdivisionViewModel = new ChoiceSubdivisionViewModel(uowFactory,UoW);
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
			{"report_date", ReportDate },
			{"show_sex", ShowSex },
			{"show_size", ShowSize },
			{"group_by_subdivision", GroupBySubdivision },
			{"subdivision_ids", ChoiceSubdivisionViewModel.SelectedChoiceSubdivisionIds() },
			{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds() },
		};

		#region Параметры
		IUnitOfWork UoW;
		private readonly FeaturesService featuresService;
		public bool SensetiveLoad => ReportDate != null;
		
		public ChoiceSubdivisionViewModel ChoiceSubdivisionViewModel;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		#endregion
		
		#region Свойства
		private DateTime? reportDate = DateTime.Today;
		public virtual DateTime? ReportDate {
			get => reportDate;
			set => SetField(ref reportDate, value);
		}

		private bool showSex;
		public virtual bool ShowSex {
			get => showSex;
			set => SetField(ref showSex, value);
		}

		private bool showSize;
		public virtual bool ShowSize {
			get => showSize;
			set => SetField(ref showSize, value);
		}
		
		private bool groupBySubdivision;
		public virtual bool GroupBySubdivision {
			get => groupBySubdivision;
			set => SetField(ref groupBySubdivision, value);
		}
		#endregion
	}
}
