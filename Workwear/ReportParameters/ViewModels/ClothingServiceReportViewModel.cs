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
		
		private ClothingServiceReportType reportType = ClothingServiceReportType.ClaimList;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		[PropertyChangedAlso(nameof(VisiblePeriodOfBegitn))]
		[PropertyChangedAlso(nameof(VisibleShowClosed))]
		[PropertyChangedAlso(nameof(VisibleShowEmployees))]
		[PropertyChangedAlso(nameof(VisibleShowPhone))]
		[PropertyChangedAlso(nameof(VisibleShowStatus))]
		[PropertyChangedAlso(nameof(VisibleShowComment))]
		[PropertyChangedAlso(nameof(VisibleShowZero))]
		[PropertyChangedAlso(nameof(Title))]
		[PropertyChangedAlso(nameof(ShowClosedLabel))]
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
			get => (ReportType == ClothingServiceReportType.ClaimList || ReportType == ClothingServiceReportType.ClaimForStatus 
						? "Заявки на обслуживание" : "") +
			       (ReportType == ClothingServiceReportType.ClaimMetric
						? "Отчёт по нахождению в обслуживании" : "") +
			        (ReportType == ClothingServiceReportType.ClaimCount
                   		? $"Количество обращений за {StartDate.ToShortDateString()}-{EndDate.ToShortDateString()}" : "") +
			        (ReportType == ClothingServiceReportType.PostamatUse
                   		? $"Использование постаматов за {StartDate.ToShortDateString()}-{EndDate.ToShortDateString()}" : "") +
			       (ShowClosed ? $" ({StartDate.ToShortDateString()}-{EndDate.ToShortDateString()})" : "") +
			       (ReportType == ClothingServiceReportType.ClaimForStatus ? $" в статусе \"{Status.GetEnumTitle()}\"" : "")+
			       $" от {DateTime.Today.ToShortDateString()}";
		}
		
		private Dictionary<string, object> SetParameters() {
			switch(ReportType) {
				case ClothingServiceReportType.ClaimList:
					return new Dictionary<string, object> {
						{ "show_closed", showClosed },
						{ "start_date", StartDate },
						{ "end_date", EndDate },
					};
				case ClothingServiceReportType.ClaimForStatus :
					return new Dictionary<string, object> {
						{ "report_name", Title },
						{ "show_comment", ShowComments },
						{ "show_phone", ShowPhone },
						{ "status", Status },
					};
				case ClothingServiceReportType.ClaimMetric:
					return new Dictionary<string, object> {
						{ "report_name", Title },
						{ "show_closed", showClosed },
						{ "start_date", StartDate },
						{ "finish_date", EndDate },
						{ "show_emoloyee", showEmployees },
					};
				case ClothingServiceReportType.ClaimCount:
					return new Dictionary<string, object> {
						{ "report_name", Title },
						{ "start_date", StartDate },
						{ "finish_date", EndDate },
						{ "show_phone", ShowPhone },
						{ "show_zero", ShowZero },
					};
				case ClothingServiceReportType.PostamatUse:
					return new Dictionary<string, object> {
						{ "report_name", Title },
						{ "start_date", StartDate },
						{ "finish_date", EndDate },
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

		public virtual string ShowClosedLabel =>
			reportType == ClothingServiceReportType.PostamatUse ? "Период:" :
				"Период поступления заявок:";
		
		private bool showClosed = false;
		[PropertyChangedAlso(nameof(VisiblePeriodOfBegitn))]
		[PropertyChangedAlso(nameof(Title))]
		public virtual bool ShowClosed {
			get => showClosed;
			set => SetField(ref showClosed, value);
		}
		
		private bool showEmployees = true;
		public virtual bool ShowEmployees {
			get => showEmployees;
			set => SetField(ref showEmployees, value);
		}
		
		private bool showZero = false;
		public virtual bool ShowZero {
			get => showZero;
			set => SetField(ref showZero, value);
		}
		
		private DateTime startDate = DateTime.Now.AddMonths(-1);
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		[PropertyChangedAlso(nameof(Title))]
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}
		
		private DateTime endDate =  DateTime.Now;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		[PropertyChangedAlso(nameof(Title))]
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		public bool SensetiveLoad => !ShowClosed || (ShowClosed && StartDate != null && EndDate != null && startDate <= endDate);
		public bool VisiblePeriodOfBegitn => reportType == ClothingServiceReportType.ClaimCount || (VisibleShowClosed && ShowClosed) || reportType == ClothingServiceReportType.PostamatUse;
		public bool VisibleShowClosed => reportType == ClothingServiceReportType.ClaimList || reportType == ClothingServiceReportType.ClaimMetric;
		public bool VisibleShowEmployees => reportType == ClothingServiceReportType.ClaimMetric;
		public bool VisibleShowPhone => reportType == ClothingServiceReportType.ClaimCount || reportType == ClothingServiceReportType.ClaimForStatus;
		public bool VisibleShowComment => reportType == ClothingServiceReportType.ClaimForStatus;
		public bool VisibleShowStatus => reportType == ClothingServiceReportType.ClaimForStatus;
		public bool VisibleShowZero => reportType == ClothingServiceReportType.ClaimCount;
	}
	public enum ClothingServiceReportType {
		[ReportIdentifier("ClothingService.ClothingServiceReport")]
		[Display(Name = "Список заявок")]
		ClaimList,
		[ReportIdentifier("ClothingService.ClothingServiceStatusReport")]
		[Display(Name = "Заявки по статусу")]
		ClaimForStatus,
        [ReportIdentifier("ClothingService.ClothingServiceMetricReport")]
        [Display(Name = "Нахождение в обслуживании")]
        ClaimMetric,
		[ReportIdentifier("ClothingService.ClothingServiceCountClaimReport")]
		[Display(Name = "Количество обращений")]
		ClaimCount,
        [ReportIdentifier("ClothingService.ClothingServicePostamatReport")]
        [Display(Name = "Использование постаматов")]
        PostamatUse
	}
	
}
