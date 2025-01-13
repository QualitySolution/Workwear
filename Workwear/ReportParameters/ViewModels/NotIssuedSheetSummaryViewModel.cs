using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.ReportParameters.ViewModels;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;

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
			var builder = new CommonEEVMBuilderFactory<NotIssuedSheetSummaryViewModel>(rdlViewerViewModel, this, UoW, navigation, autofacScope);
			
			SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
				.MakeByType()
				.Finish();
			DepartmentEntry = builder.ForProperty(x => x.Department)
				.MakeByType()
				.Finish();
			DepartmentEntry.EntitySelector = new DepartmentJournalViewModelSelector(rdlViewerViewModel, navigation, SubdivisionEntry);
			
			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(UoW);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
			ChoiceEmployeeGroupViewModel = new ChoiceEmployeeGroupViewModel(UoW);
			ChoiceEmployeeGroupViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;

			excludeInVacation = true;
			condition = true;
			
			warehousesList.Add(new Warehouse(){Id = -2, Name = "Не показывать"}); //По умолчанию
			var whs = UoW.GetAll<Warehouse>().ToList();
			if(!featuresService.Available(WorkwearFeature.Warehouses) || whs.Count == 1) 
				warehousesList.Add(new Warehouse(){Id = -1, Name = "Показать"});
			else {
				warehousesList.AddRange(whs);
				warehousesList.Add(new Warehouse(){Id = -1, Name = "На всех складах"});
			}
			warehouse = warehousesList.First();
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"report_date", ReportDate },
					{"subdivision_id", Subdivision?.Id ?? -1 },
					{"department_id", Department?.Id ?? -1},
					{"department_name", Department?.Name},
					{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedIds.Length == 0 ? 
						new [] {-1} :
						ChoiceProtectionToolsViewModel.SelectedIds },
					{"without_groups", ChoiceEmployeeGroupViewModel.NullIsSelected },	
					{"employee_groups_ids", ChoiceEmployeeGroupViewModel.SelectedIdsMod},
					{"issue_type", IssueType?.ToString() },
					{"group_by_subdivision", GroupBySubdivision },
					{"show_sex", ShowSex },
					{"show_employees", ShowEmployees },
					{"exclude_in_vacation", ExcludeInVacation },
					{"condition", Condition },
					{"exclude_before", ExcludeBefore },
					{"show_stock", Warehouse.Id != -2},
					{"all_warehouse", Warehouse.Id == -1},
					{"warehouse_id", Warehouse.Id },
					{"hide_worn", HideWorn},
				 };

		#region Параметры
		
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}
		
		private NotIssuedSheetSummaryReportType reportType;
		public virtual NotIssuedSheetSummaryReportType ReportType {
			get => reportType;
			set {
				SetField(ref reportType, value);
				OnPropertyChanged(nameof(VisibleShowEmployees));
			}
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

		private Warehouse warehouse;
		[PropertyChangedAlso(nameof(StockElementsSensetive))]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value); }
		}

		private List<Warehouse> warehousesList = new List<Warehouse>();
		public virtual List<Warehouse> WarehousesList {
			get { return warehousesList; }
			set { SetField(ref warehousesList, value); }
		}

		private bool hideWorn;
		public virtual bool HideWorn {
			get => warehouse.Id != -2 && hideWorn;
			set => SetField(ref hideWorn, value);
		}
		
		private bool showEmployees;
		public virtual bool ShowEmployees {
			get => showEmployees;
			set => SetField(ref showEmployees, value);
		}
		
		private Subdivision subdivision;
		[PropertyChangedAlso(nameof(GroupByElementSensetive))]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set {
				SetField(ref subdivision, value);
				GroupBySubdivision = Subdivision != null;
				if(Department != null && Department.Subdivision != Subdivision)
					Department = null;
			}
		}
		
		private Department department;
		public virtual Department Department {
			get => department;
			set {
				if(SetField(ref department, value)) {
					if(department != null && !DomainHelper.EqualDomainObjects(Subdivision, department?.Subdivision))
						Subdivision = department?.Subdivision;
				}
			}
		}
		#endregion
		
		#region Свойства
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool VisibleCondition => featuresService.Available(WorkwearFeature.ConditionNorm);
		public bool VisibleChoiceEmployeeGroup => featuresService.Available(WorkwearFeature.EmployeeGroups);
		public bool SensetiveLoad => ReportDate != null && !ChoiceProtectionToolsViewModel.AllUnSelected && !ChoiceEmployeeGroupViewModel.AllUnSelected;
		public bool VisibleShowEmployees => ReportType == NotIssuedSheetSummaryReportType.Flat;
		public bool StockElementsSensetive => warehouse.Id != -2;

		public bool GroupByElementSensetive => Subdivision == null;
		
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
			if(nameof(ChoiceEmployeeGroupViewModel.AllUnSelected)==e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public EntityEntryViewModel<Department> DepartmentEntry;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		public ChoiceEmployeeGroupViewModel ChoiceEmployeeGroupViewModel;
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
