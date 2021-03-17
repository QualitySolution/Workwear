using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.Report;
using QS.Report.ViewModels;

namespace workwear.ReportParameters.ViewModels.Gok
{
	public class IssuanceSheet138ViewModel : ReportParametersViewModelBase
	{
		public IssuanceSheet138ViewModel(RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel)
		{
			Title = "Ведомость №138";
			Identifier = "Gok.IssuanceSheet138";
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"dateStart", StartDate },
					{"dateEnd", EndDate},
				 };

		public override ReportInfo ReportInfo {
			get {
				var info = base.ReportInfo;
				info.ParameterDatesWithTime = false;
				return info;
			}
		}

		#region Параметры
		private DateTime? startDate;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		#endregion
		#region Свойства
		public bool SensetiveLoad => StartDate != null && EndDate != null;
		#endregion
	}
}
