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
		};
		
		private DateTime? startDate=  DateTime.Now.AddMonths(-1);
		private DateTime? endDate =  DateTime.Now;
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}
		public bool SensetiveLoad => (StartDate != null && EndDate != null && startDate <= endDate);
	}
}
