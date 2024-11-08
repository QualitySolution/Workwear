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
			UoW = uowFactory.CreateWithoutRoot();
			var builder = new CommonEEVMBuilderFactory<NotIssuedSheetViewModel>(rdlViewerViewModel, this, UoW, navigation, autofacScope);
			
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
					{"protection_tools_ids", ChoiceProtectionToolsViewModel.SelectedIdsMod},
					{"without_groups", ChoiceEmployeeGroupViewModel.NullIsSelected },	
					{"employee_groups_ids", ChoiceEmployeeGroupViewModel.SelectedIdsMod},
					{"issue_type", IssueType?.ToString() },
					{"exclude_before", ExcludeBefore },
					{"exclude_in_vacation", ExcludeInVacation },
					{"condition", Condition },
					{"show_stock", Warehouse.Id != -2},
					{"all_warehouse", Warehouse.Id == -1},
					{"warehouse_id", Warehouse.Id },
					{"exclude_zero_stock", ExcludeZeroStock},
					{"hide_worn", HideWorn},
					{"view_phones", ViewPhones},
				 };

		#region Параметры
		
		public override string Identifier { 
			get => ReportType.GetAttribute<ReportIdentifierAttribute>().Identifier;
			set => throw new InvalidOperationException();
		}
		
		private NotIssuedSheetReportType reportType;
		[PropertyChangedAlso(nameof(VisibleChoiceEmployeeGroup))]
		public virtual NotIssuedSheetReportType ReportType {
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
		
		private bool viewPhones;
		public virtual bool ViewPhones {
			get => viewPhones;
			set => SetField(ref viewPhones, value);
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

		private bool excludeZeroStock;
		public virtual bool ExcludeZeroStock {
			get =>  warehouse.Id != -2 && excludeZeroStock;
			set => SetField(ref excludeZeroStock, value);
		}
		
		private bool hideWorn;
		public virtual bool HideWorn {
			get =>  warehouse.Id != -2 && hideWorn;
			set => SetField(ref hideWorn, value);
		}
		#endregion
		
		#region Свойства
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool VisibleCondition => featuresService.Available(WorkwearFeature.ConditionNorm);
		public bool VisibleChoiceEmployeeGroup => featuresService.Available(WorkwearFeature.EmployeeGroups);
		public bool SensetiveLoad => ReportDate != null && !ChoiceProtectionToolsViewModel.AllUnSelected && !ChoiceEmployeeGroupViewModel.AllUnSelected;
		public object StockElementsSensetive => warehouse.Id != -2;

		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
			if(nameof(ChoiceEmployeeGroupViewModel.AllUnSelected)==e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		
		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set {
				SetField(ref subdivision, value);
				if(Department != null && Department.Subdivision != Subdivision)
					Department = null;
			}
		}
		
		private Department department;
		public virtual Department Department {
			get => department;
			set {
				if(SetField(ref department, value))
					if(department != null && !DomainHelper.EqualDomainObjects(Subdivision, department?.Subdivision))
						Subdivision = department?.Subdivision;
			}
		}
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public EntityEntryViewModel<Department> DepartmentEntry;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		public ChoiceEmployeeGroupViewModel ChoiceEmployeeGroupViewModel;
		private readonly FeaturesService featuresService;
		#endregion

		public void Dispose()
		{
			UoW.Dispose();
		}
	}

	public enum NotIssuedSheetReportType {
		[ReportIdentifier("NotIssuedSheet")]
		[Display(Name = "Форматировано")]
		Common,
		[ReportIdentifier("NotIssuedSheetFlat")]
		[Display(Name = "Только данные")]
		Flat
	}
}
