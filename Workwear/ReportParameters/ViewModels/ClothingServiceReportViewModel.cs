using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.DomainModel.Entity;

namespace Workwear.ReportParameters.ViewModels {
	public class ClothingServiceReportViewModel: ReportParametersViewModelBase {

		public ClothingServiceReportViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel)
		{
			Title = "Обслуживание одежды";
			Identifier = "ClothingServiceReport";
		}
		protected override Dictionary<string, object> Parameters => SetParameters();
		
		private Dictionary<string, object> SetParameters() {
			var parameters = new Dictionary<string, object>();
			using (var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot()) {
				parameters.Add("show_closed", showClosed);
				parameters.Add("start_date", StartDate ?? DateTime.MinValue);
				parameters.Add("end_date", EndDate ?? DateTime.MaxValue);
			}
			return parameters;
		}
		
		private bool showClosed = false;
		[PropertyChangedAlso(nameof(VisibleUseAlternative))]
		public virtual bool ShowClosed {
			get => showClosed;
			set {
				if(SetField(ref showClosed, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}
		public bool VisibleUseAlternative => ShowClosed;
		private DateTime? startDate=  DateTime.Now.AddMonths(-1);
		

		private DateTime? endDate =  DateTime.Now;

		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value))
					OnPropertyChanged(nameof(SensetiveLoad));
			}
		}

		public bool SensetiveLoad => !ShowClosed || (ShowClosed && StartDate != null && EndDate != null && startDate <= endDate);
		
	}
	
}
