using QS.Report.ViewModels;
using QS.ViewModels.Extension;
using System.Collections.Generic;
using System;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ReportParameters.ViewModels {
	public class WriteOffActViewModel: ReportParametersViewModelBase, IDialogDocumentation {
		private readonly FeaturesService featuresService;
		
		public WriteOffActViewModel(RdlViewerViewModel rdlViewerViewModel, FeaturesService featuresService) : base(rdlViewerViewModel) 
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			Title = "Справка по списаниям";
			Identifier = "WriteOffAct";
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("reports.html#written-off");
		public string ButtonTooltip => DocHelper.GetReportDocTooltip(Title);
		#endregion

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{ "start_date", StartDate },
			{ "end_date", EndDate },
			{ "report_date", ReportDate},
			{"doc_write_off", ShowDocWriteOff},
			{"auto_write_off", ShowAutoWriteOff},
			{"income", ShowIncome},
			{"service_detail", ShowServiceDetail}
		};
		
		private DateTime? startDate=  DateTime.Today.AddMonths(-1);
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}
		private DateTime? endDate =  DateTime.Today;
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}
		private DateTime? reportDate = DateTime.Today;
		public virtual DateTime? ReportDate => reportDate;

		private bool showDocWriteOff=true;
		public virtual bool ShowDocWriteOff {
			get=> showDocWriteOff;
			set {
				if(SetField(ref showDocWriteOff, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}
		private bool showAutoWriteOff=false;
		public virtual bool ShowAutoWriteOff {
			get=> showAutoWriteOff;
			set {
				if(SetField(ref showAutoWriteOff, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}
		private bool showIncome=true;
		public virtual bool ShowIncome {
			get=> showIncome;
			set {
				if(SetField(ref showIncome, value))
					OnPropertyChanged(nameof(SensitiveLoad));
			}
		}

		private bool showServiceDetail = false;
		public virtual bool ShowServiceDetail {
			get => showServiceDetail;
			set => SetField(ref showServiceDetail, value);
		}

		public bool VisibleServiceDetail => featuresService.Available(WorkwearFeature.ClothingService);

		public bool SensitiveLoad => (StartDate != null && EndDate != null && startDate <= endDate) && ReportDate != null && 
		                             (showDocWriteOff || showIncome || showAutoWriteOff);
	}
}
