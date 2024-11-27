using QS.Report.ViewModels;
using System.Collections.Generic;
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
			{ "report_date", ReportDate},
			{"doc_write_off", ShowDocWriteOff},
			{"auto_write_off", ShowAutoWriteOff},
			{"income", ShowIncome}
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
		private bool showAutoWriteOff=true;
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

		public bool SensitiveLoad => (StartDate != null && EndDate != null && startDate <= endDate) && ReportDate != null && 
		                             (showDocWriteOff || showIncome || showAutoWriteOff);
	}
}
