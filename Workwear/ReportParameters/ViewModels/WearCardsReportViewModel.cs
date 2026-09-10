using QS.Report.ViewModels;
using QS.ViewModels.Extension;
using System;
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
			{"only_working", OnlyWorking},
			{"actual_date", ActualDate},
			{"only_hired", OnlyHired},
			{"hire_from", HirePeriodStart},
			{"hire_to", HirePeriodEnd},
			{"only_dismissed", OnlyDismissed},
			{"dismiss_from", DismissPeriodStart},
			{"dismiss_to", DismissPeriodEnd},
			{"only_without_norms", WithoutNorms},
			{"only_with_norms", WithNorms},
			{"show_phone", ShowPhone},
		};

		#region Блок 1: статус сотрудника

		private bool onlyWorking = true;
		public virtual bool OnlyWorking {
			get => onlyWorking;
			set => SetField(ref onlyWorking, value);
		}

		private DateTime actualDate = DateTime.Today;
		public virtual DateTime ActualDate {
			get => actualDate;
			set => SetField(ref actualDate, value);
		}

		private bool onlyHired;
		public virtual bool OnlyHired {
			get => onlyHired;
			set => SetField(ref onlyHired, value);
		}

		private DateTime hirePeriodStart = DateTime.Today.AddYears(-1);
		public virtual DateTime HirePeriodStart {
			get => hirePeriodStart;
			set => SetField(ref hirePeriodStart, value);
		}

		private DateTime hirePeriodEnd = DateTime.Today;
		public virtual DateTime HirePeriodEnd {
			get => hirePeriodEnd;
			set => SetField(ref hirePeriodEnd, value);
		}

		private bool onlyDismissed;
		public virtual bool OnlyDismissed {
			get => onlyDismissed;
			set => SetField(ref onlyDismissed, value);
		}

		private DateTime dismissPeriodStart = DateTime.Today.AddYears(-1);
		public virtual DateTime DismissPeriodStart {
			get => dismissPeriodStart;
			set => SetField(ref dismissPeriodStart, value);
		}

		private DateTime dismissPeriodEnd = DateTime.Today;
		public virtual DateTime DismissPeriodEnd {
			get => dismissPeriodEnd;
			set => SetField(ref dismissPeriodEnd, value);
		}

		#endregion

		#region Блок 2: нормы

		private bool withoutNorms = true;
		public virtual bool WithoutNorms {
			get => withoutNorms;
			set => SetField(ref withoutNorms, value);
		}

		private bool withNorms = true;
		public virtual bool WithNorms {
			get => withNorms;
			set => SetField(ref withNorms, value);
		}

		#endregion

		#region Блок 3: отображаемые колонки

		private bool showPhone;
		public virtual bool ShowPhone {
			get => showPhone;
			set => SetField(ref showPhone, value);
		}

		#endregion
	}
}
