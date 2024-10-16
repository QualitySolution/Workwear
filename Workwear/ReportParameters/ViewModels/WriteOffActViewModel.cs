using QS.Report.ViewModels;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.DomainModel.Entity;
using System;

namespace Workwear.ReportParameters.ViewModels {
	public class WriteOffActViewModel: ReportParametersViewModelBase {
		public WriteOffActViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel) 
		{
			Title = "Отчёт по актам списания";
			Identifier = "WriteOffAct";
		}
		protected override Dictionary<string, object> Parameters => SetParameters();

		private Dictionary<string, object> SetParameters() {
			var parameters = new Dictionary<string, object>();
			using (var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot()) {
				parameters.Add("start_date", StartDate ?? DateTime.MinValue);
				parameters.Add("end_date", EndDate ?? DateTime.MaxValue);
			}
			return parameters;
		}
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
		public bool SensetiveLoad => (StartDate != null && EndDate != null && startDate <= endDate);
	}
}
