using System;
using QS.DomainModel.Entity;
using QS.Report.ViewModels;
using QS.ViewModels.Extension;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.Report;
using Workwear.Domain.ClothingService;
using Workwear.Tools;

namespace Workwear.ReportParameters.ViewModels {
	public class ClothingServiceReportViewModel: ReportParametersViewModelBase, IDialogDocumentation {

		public ClothingServiceReportViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel) {
		}
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("reports.html#report-service-claims");
		public string ButtonTooltip => DocHelper.GetReportDocTooltip(Title);
		#endregion
		
		private ClothingServiceReportType reportType;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		[PropertyChangedAlso(nameof(VisiblePeriodOfBegitn))]
		[PropertyChangedAlso(nameof(VisibleShowClosed))]
		[PropertyChangedAlso(nameof(VisibleShowPhone))]
		[PropertyChangedAlso(nameof(VisibleShowStatus))]
		[PropertyChangedAlso(nameof(VisibleShowComment))]
		public virtual ClothingServiceReportType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value);
			}
		}
		protected override Dictionary<string, object> Parameters => SetParameters();
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
		}
		public override string Title {
			get => "Заявки на обслуживание" +
			       (ReportType == ClothingServiceReportType.ClaimForStatus ? $" в статусе \"{Status.GetEnumTitle()}\"" : "")+
			       $" от {DateTime.Today.ToShortDateString()}";
		}

		private Dictionary<string, object> SetParameters() {
			switch(ReportType) {
				case ClothingServiceReportType.ClaimList:
					return new Dictionary<string, object> {
						{ "show_closed", showClosed },
						{ "start_date", StartDate ?? DateTime.MinValue },
						{ "end_date", EndDate ?? DateTime.MaxValue },
					};
				case ClothingServiceReportType.ClaimForStatus :
					return new Dictionary<string, object> {
						{ "report_name", Title },
						{ "show_comment", ShowComments },
						{ "show_phone", ShowPhone },
						{ "status", Status },
					};
				default: throw new InvalidOperationException(nameof(SetParameters));
			}
		}
		
		private ClaimState status = ClaimState.InWashing;
		public virtual ClaimState Status {
			get => status;
			set => SetField(ref status, value);
		}
		
		private bool showComments = false;
		public virtual bool ShowComments {
			get => showComments;
			set => SetField(ref showComments, value);
		}
		
		private bool showPhone = false;
		public virtual bool ShowPhone {
			get => showPhone;
			set => SetField(ref showPhone, value);
		}

		private bool showClosed = false;
		[PropertyChangedAlso(nameof(VisiblePeriodOfBegitn))]
		public virtual bool ShowClosed {
			get => showClosed;
			set => SetField(ref showClosed, value);
		}
		
		private DateTime? startDate = DateTime.Now.AddMonths(-1);
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}
		
		private DateTime? endDate =  DateTime.Now;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		public bool SensetiveLoad => !ShowClosed || (ShowClosed && StartDate != null && EndDate != null && startDate <= endDate);
		public bool VisiblePeriodOfBegitn => VisibleShowClosed && ShowClosed;
		public bool VisibleShowClosed => reportType == ClothingServiceReportType.ClaimList;
		public bool VisibleShowPhone => reportType == ClothingServiceReportType.ClaimForStatus;
		public bool VisibleShowComment => reportType == ClothingServiceReportType.ClaimForStatus;
		public bool VisibleShowStatus => reportType == ClothingServiceReportType.ClaimForStatus;
	}
	public enum ClothingServiceReportType {
		[ReportIdentifier("ClothingServiceReport")]
		[Display(Name = "Список заявок")]
		ClaimList,
		[ReportIdentifier("ClothingServiceStatusReport")]
		[Display(Name = "Заявки по статусу")]
		ClaimForStatus
	}
	
}
