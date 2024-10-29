using QS.Report.ViewModels;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.DomainModel.Entity;
using System;

namespace Workwear.ReportParameters.ViewModels {
	public class WriteOffActViewModel: ReportParametersViewModelBase {
		public WriteOffActViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel) 
		{
			Title = "Справка по списаниям";
			Identifier = "WriteOffAct";
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>() {
			{ "start_date", StartDate },
			{ "end_date", EndDate },
			{ "report_date", ReportDate}
		};
		
		private DateTime? startDate=  DateTime.Now.AddMonths(-1);
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}
		private DateTime? endDate =  DateTime.Now;
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}
		private DateTime? reportDate = DateTime.Today;
		public virtual DateTime? ReportDate {
			get => reportDate;
		}
		
		public bool SensetiveLoad => (StartDate != null && EndDate != null && startDate <= endDate) && ReportDate != null;
	}
}
