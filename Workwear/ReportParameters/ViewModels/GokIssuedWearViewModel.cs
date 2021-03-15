using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using workwear.Repository.Company;

namespace workwear.ReportParameters.ViewModels
{
	public class GokIssuedWearViewModel : ReportParametersViewModelBase
	{
		public GokIssuedWearViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, SubdivisionRepository subdivisionRepository) : base(rdlViewerViewModel)
		{
			Title = "Сводная ведомость по СИЗ";
			Identifier = "Gok.IssuedWear";
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
