using System;
using System.Collections.Generic;
using System.Linq;
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

namespace workwear.ReportParameters.ViewModels {
	public class RequestSheetViewModel : ReportParametersViewModelBase, IDisposable
	{
		private readonly FeaturesService featuresService;

		public RequestSheetViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			FeaturesService featuresService)
			: base(rdlViewerViewModel)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			Title = "Заявка на спецодежду";
			Identifier = "RequestSheet";

			uow = uowFactory.CreateWithoutRoot();
			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, uow, navigation, autofacScope);

			EntrySubdivisionViewModel = builder.ForEntity<Subdivision>().MakeByType().Finish();
			var defaultMonth = DateTime.Today.AddMonths(1);
			BeginMonth = EndMonth = defaultMonth.Month;
			BeginYear = EndYear = defaultMonth.Year;

			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(uow);
		}

		private readonly IUnitOfWork uow;

		#region Entry
		public readonly EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		#endregion

		#region Свойства View
		private int beginMonth;
		[PropertyChangedAlso(nameof(SensitiveRunReport))]
		public virtual int BeginMonth {
			get => beginMonth;
			set => SetField(ref beginMonth, value);
		}

		private int beginYear;
		[PropertyChangedAlso(nameof(SensitiveRunReport))]
		public virtual int BeginYear {
			get => beginYear;
			set => SetField(ref beginYear, value);
		}

		private int endMonth;
		[PropertyChangedAlso(nameof(SensitiveRunReport))]
		public virtual int EndMonth {
			get => endMonth;
			set => SetField(ref endMonth, value);
		}

		private int endYear;
		[PropertyChangedAlso(nameof(SensitiveRunReport))]
		public virtual int EndYear {
			get => endYear;
			set => SetField(ref endYear, value);
		}

		private IssueType? issueTypeOptions;
		public IssueType? IssueTypeOptions {
			get => issueTypeOptions;
			set => SetField(ref issueTypeOptions, value);
		}

		private bool addChildSubdivisions;
		public bool AddChildSubdivisions {
			get => addChildSubdivisions;
			set => SetField(ref addChildSubdivisions, value);
		}
		
		private bool showSex;
		public bool ShowSex {
			get => showSex;
			set => SetField(ref showSex, value);
		}
		
		private bool excludeInVacation;
		public virtual bool ExcludeInVacation {
			get => excludeInVacation;
			set => SetField(ref excludeInVacation, value);
		}

		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool SensitiveRunReport => new DateTime(BeginYear, BeginMonth, 1) <= new DateTime(EndYear, EndMonth, 1);
		#endregion

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"begin_month", BeginMonth },
					{"begin_year", BeginYear},
					{"end_month", EndMonth},
					{"end_year", EndYear},
					{"subdivisions", SelectSubdivisions() },
					{"issue_type", IssueTypeOptions?.ToString() },
					{"protectionTools", ChoiceProtectionToolsViewModel.SelectedProtectionToolsIds() },
					{"headSubdivision", EntrySubdivisionViewModel.Entity?.Id ?? -1},
					{"show_sex", ShowSex },
					{"exclude_in_vacation", excludeInVacation }
					};

		public void Dispose()
		{
			uow.Dispose();
		}

		private int[] SelectSubdivisions() {
			if(EntrySubdivisionViewModel.Entity is null) 
				return new[] {-1};
			if (!AddChildSubdivisions)
				return new[] {EntrySubdivisionViewModel.Entity.Id}; 
			return EntrySubdivisionViewModel.Entity
				.AllGenerationsSubdivisions.Take(500)
				.Select(x => x.Id)
				.Distinct()
				.ToArray();
		}
	}
}
