using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using Workwear.ReportParameters.ViewModels;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;

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
			var builder = new CommonEEVMBuilderFactory<RequestSheetViewModel>
				(rdlViewerViewModel, this, uow, navigation, autofacScope);

			EntrySubdivisionViewModel = builder.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.Finish();
			EntryDepartmentViewModel = builder.ForProperty(x => x.Department)
				.UseViewModelJournalAndAutocompleter<DepartmentJournalViewModel>()
				.Finish();
			EntryDepartmentViewModel.EntitySelector =
				new DepartmentJournalViewModelSelector(rdlViewerViewModel, navigation, EntrySubdivisionViewModel);
			
			var defaultMonth = DateTime.Today.AddMonths(1);
			BeginMonth = EndMonth = defaultMonth.Month;
			BeginYear = EndYear = defaultMonth.Year;

			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(uow);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
			ChoiceEmployeeGroupViewModel = new ChoiceEmployeeGroupViewModel(uow);
			ChoiceEmployeeGroupViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
		}

		private readonly IUnitOfWork uow;
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensitiveRunReport));
			if(nameof(ChoiceEmployeeGroupViewModel.AllUnSelected)==e.PropertyName)
				OnPropertyChanged(nameof(SensitiveRunReport));
			
		}

		#region Entry
		public readonly EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;
		public readonly EntityEntryViewModel<Department> EntryDepartmentViewModel;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		public ChoiceEmployeeGroupViewModel ChoiceEmployeeGroupViewModel;
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

		private Subdivision subdivision;
		public Subdivision Subdivision{
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		public Department department;
		[PropertyChangedAlso(nameof(SensetiveSubdivision))]
		public Department Department {
			get => department;
			set {
				if(SetField(ref department, value)) {
					Subdivision = department?.Subdivision ?? subdivision;
					if(department != null)
						AddChildSubdivisions = false;
				}
			}
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

		private bool showSize;
		public virtual bool ShowSize {
			get => showSize;
			set=>SetField(ref showSize, value);
		}
		
		public bool SensetiveSubdivision => department == null;
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool SensitiveRunReport => new DateTime(BeginYear, BeginMonth, 1) <= new DateTime(EndYear, EndMonth, 1)
		                                  && !ChoiceProtectionToolsViewModel.AllUnSelected && !ChoiceEmployeeGroupViewModel.AllUnSelected;

		public bool VisibleChoiceEmployeeGroup => featuresService.Available(WorkwearFeature.EmployeeGroups);
		#endregion

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"begin_month", BeginMonth },
					{"begin_year", BeginYear},
					{"end_month", EndMonth},
					{"end_year", EndYear},
					{"subdivisions", SelectSubdivisions() },
					{"department_id", Department?.Id ?? -1},
					{"issue_type", IssueTypeOptions?.ToString() },
					{"protectionTools", ChoiceProtectionToolsViewModel.SelectedIdsMod},
					{"headSubdivision", EntrySubdivisionViewModel.Entity?.Id ?? -1},
					{"show_sex", ShowSex },
					{"exclude_in_vacation", excludeInVacation },
					{"without_groups", ChoiceEmployeeGroupViewModel.NullIsSelected},
					{"employee_groups_ids", ChoiceEmployeeGroupViewModel.SelectedIdsMod},
					{"show_size", ShowSize},
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
