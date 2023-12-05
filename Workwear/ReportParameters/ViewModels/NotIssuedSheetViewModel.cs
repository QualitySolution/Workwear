using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.ReportParameters.ViewModels;
using Workwear.Tools.Features;

namespace workwear.ReportParameters.ViewModels
{
	public class NotIssuedSheetViewModel : ReportParametersViewModelBase, IDisposable
	{
		IUnitOfWork UoW;

		public NotIssuedSheetViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			FeaturesService featuresService)
			: base(rdlViewerViewModel)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));

			Title = "Справка по невыданному (Детально)";
			Identifier = "NotIssuedSheet";

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
					{"subdivision_id", SubdivisionEntry.Entity?.Id ?? -1 },
					{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds.Length == 0 ? 
						new [] {-1} :
						ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds },
					{"issue_type", IssueType?.ToString() },
					{"exclude_before", ExcludeBefore },
					{"exclude_in_vacation", ExcludeInVacation },
					{"condition", Condition },
					{"show_stock", ShowStock},
					{"exclude_zero_stock", ExcludeZeroStock},
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
		
		private bool condition;
		public virtual bool Condition {
			get => condition;
			set => SetField(ref condition, value);
		}
		
		private bool showStock;
		[PropertyChangedAlso(nameof(DontShowZeroStockVisible))]
		public virtual bool ShowStock {
			get => showStock;
			set {
				SetField(ref showStock, value);
				if(!value) //Сброс при снятии
					ExcludeZeroStock = false;
			} 
			
		}

		private bool excludeZeroStock;
		public virtual bool ExcludeZeroStock {
			get => showStock && excludeZeroStock;
			set => SetField(ref excludeZeroStock, value);
		}
		#endregion
		#region Свойства
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool VisibleCondition => featuresService.Available(WorkwearFeature.ConditionNorm);
		public bool SensetiveLoad => ReportDate != null && !ChoiceProtectionToolsViewModel.AllUnSelected;
		public object DontShowZeroStockVisible => ShowStock;

		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		private readonly FeaturesService featuresService;
		#endregion

		public void Dispose()
		{
			UoW.Dispose();
		}
	}
}
