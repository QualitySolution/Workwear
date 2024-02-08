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
					{"subdivision_id", SubdivisionEntry.Entity == null ? -1 : SubdivisionEntry.Entity.Id },
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
		#endregion
		#region Свойства
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool VisibleCondition => featuresService.Available(WorkwearFeature.ConditionNorm);
		public bool VisibleChoiceEmployeeGroup => featuresService.Available(WorkwearFeature.EmployeeGroups);
		public bool SensetiveLoad => ReportDate != null && !ChoiceProtectionToolsViewModel.AllUnSelected;
		public object StockElementsSensetive => warehouse.Id != -2;
		
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(ChoiceProtectionToolsViewModel.AllUnSelected) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveLoad));
		}
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
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
