using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Autofac;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.ReportParameters.ViewModels;
using Workwear.Tools.Features;

namespace workwear.ReportParameters.ViewModels
{
	public class NotIssuedSheetSummaryViewModel : ReportParametersViewModelBase, IDisposable
	{
		private readonly FeaturesService featuresService;
		IUnitOfWork UoW;

		public NotIssuedSheetSummaryViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			FeaturesService featuresService)
			: base(rdlViewerViewModel)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));

			Title = "Справка по невыданному (Суммарно)";
			UoW = uowFactory.CreateWithoutRoot();

			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, UoW, navigation, autofacScope);
			SubdivisionEntry = builder.ForEntity<Subdivision>().MakeByType().Finish();
			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(UoW);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;

			excludeInVacation = true;
			condition = true;
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"report_date", ReportDate },
					{"subdivision_id", SubdivisionEntry.Entity == null ? -1 : SubdivisionEntry.Entity.Id },
					{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds.Length == 0 ? 
						new [] {-1} :
						ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds },
					{"issue_type", IssueType?.ToString() },
					{"group_by_subdivision", GroupBySubdivision },
					{"show_sex", ShowSex },
					{"show_employees", ShowEmployees },
					{"exclude_in_vacation", ExcludeInVacation },
					{"condition", Condition },
					{"exclude_before", ExcludeBefore },
					{"show_stock", ShowStock},
				 };

		#region Параметры
		
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}
		
		private NotIssuedSheetSummaryReportType reportType;
		public virtual NotIssuedSheetSummaryReportType ReportType {
			get => reportType;
			set => SetField(ref reportType, value);
		}
		
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
		
		private bool condition;
		public virtual bool Condition {
			get => condition;
			set => SetField(ref condition, value);
		}
		
		private bool groupBySubdivision;
		public virtual bool GroupBySubdivision {
			get => groupBySubdivision;
			set => SetField(ref groupBySubdivision, value);
		}

		private bool showSex;
		public bool ShowSex {
			get => showSex;
			set => SetField(ref showSex, value);
		}

		private bool showStock;
		public virtual bool ShowStock {
			get => showStock;
			set => SetField(ref showStock, value);
		}
		
		private bool showEmployees;
		public virtual bool ShowEmployees {
			get => showEmployees;
			set => SetField(ref showEmployees, value);
		}
		#endregion
		#region Свойства
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool VisibleCondition => featuresService.Available(WorkwearFeature.ConditionNorm);
		public bool SensetiveLoad => ReportDate != null && !ChoiceProtectionToolsViewModel.AllUnSelected;
		
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		#endregion

		public void Dispose()
		{
			UoW.Dispose();
		}
	}
	
	public enum NotIssuedSheetSummaryReportType {
		[ReportIdentifier("NotIssuedSheetSummary")]
		[Display(Name = "Форматировано")]
		Common,
		[ReportIdentifier("NotIssuedSheetSummaryFlat")]
		[Display(Name = "Только данные")]
		Flat
	}
}
