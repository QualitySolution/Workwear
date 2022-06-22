using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;
using workwear.Domain.Stock;

namespace workwear.ReportParameters.ViewModels
{
	public class NotIssuedSheetSummaryViewModel : ReportParametersViewModelBase, IDisposable
	{
		IUnitOfWork UoW;

		public NotIssuedSheetSummaryViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, INavigationManager navigation, ILifetimeScope autofacScope) : base(rdlViewerViewModel)
		{
			Title = "Справка по невыданному (Суммарно)";
			Identifier = "NotIssuedSheetSummary";

			UoW = uowFactory.CreateWithoutRoot();

			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, UoW, navigation, autofacScope);
			SubdivisionEntry = builder.ForEntity<Subdivision>().MakeByType().Finish();

			excludeInVacation = true;
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"report_date", ReportDate },
					{"subdivision_id", SubdivisionEntry.Entity == null ? -1 : SubdivisionEntry.Entity.Id },
					{"issue_type", IssueType?.ToString() },
					{"show_sex", ShowSex },
					{"exclude_in_vacation", excludeInVacation },
					{"exclude_before", ExcludeBefore }
				 };

		#region Параметры
		private DateTime? reportDate = DateTime.Today;
		[PropertyChangedAlso(nameof(SensetiveLoad))]
		public virtual DateTime? ReportDate {
			get => reportDate;
			set => SetField(ref reportDate, value);
		}
		
		private DateTime? excludeBefore;
		public virtual DateTime? ExcludeBefore {
			get => excludeBefore;
			set => SetField(ref excludeBefore, value);
		}

		private IssueType? issueType;
		public virtual IssueType? IssueType {
			get => issueType;
			set => SetField(ref issueType, value);
		}

		private bool excludeInVacation;
		public virtual bool ExcludeInVacation {
			get => excludeInVacation;
			set => SetField(ref excludeInVacation, value);
		}

		public bool ShowSex { get; set; }
		#endregion
		#region Свойства
		public bool SensetiveLoad => ReportDate != null;
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		#endregion

		public void Dispose()
		{
			UoW.Dispose();
		}

		public void ClearExcludeBefore() => ExcludeBefore = null;
	}
}
