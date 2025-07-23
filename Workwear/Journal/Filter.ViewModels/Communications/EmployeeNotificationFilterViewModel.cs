using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace workwear.Journal.Filter.ViewModels.Communications {
	public class EmployeeNotificationFilterViewModel : JournalFilterViewModelBase<EmployeeNotificationFilterViewModel>
	{
		#region Ограничения
		private bool showOnlyWork = true;
		public virtual bool ShowOnlyWork {
			get => showOnlyWork;
			set => SetField(ref showOnlyWork, value);
		}

		private bool showOnlyLk = true;
		public virtual bool ShowOnlyLk {
			get => showOnlyLk;
			set => SetField(ref showOnlyLk, value);
		}

		private bool showOverdue = true;
		public virtual bool ShowOverdue {
			get => showOverdue;
			set => SetField(ref showOverdue, value);
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private AskIssueType isueType;
		public AskIssueType IsueType {
			get => isueType;
			set => SetField(ref isueType, value);
		}

		private SexType sexType;
		public SexType SexType {
			get => sexType;
			set => SetField(ref sexType, value);
		}

		private DateTime startDateIssue;
		public DateTime StartDateIssue {
			get => startDateIssue;
			set => SetField(ref startDateIssue, value);
		}
		
		private DateTime endDateIssue;
		public DateTime EndDateIssue {
			get => endDateIssue;
			set => SetField(ref endDateIssue, value);
		}

		private bool containsPeriod;
		[PropertyChangedAlso(nameof(PeriodSensitive))]
		[PropertyChangedAlso(nameof(WarehousesSensitive))]
		public bool ContainsPeriod {
			get => containsPeriod;
			set {
				if (value) ContainsDateBirthPeriod = false;
				SetField(ref containsPeriod, value);
			}
		}

		private bool checkInInStockAvailability;
		[PropertyChangedAlso(nameof(WarehousesSensitive))]
		public bool CheckInStockAvailability 
		{
			get => checkInInStockAvailability;
			set => SetField(ref checkInInStockAvailability, value); 
		}

		public List<Warehouse> Warehouses { get; set; }

		private Warehouse selectedWarehouse;
		public Warehouse SelectedWarehouse 
		{
			get => selectedWarehouse;
			set => SetField(ref selectedWarehouse, value);
		}
		
		public bool PeriodSensitive => ContainsPeriod;
		public bool SensitiveDateBirth => ContainsDateBirthPeriod;
		public bool WarehousesSensitive => PeriodSensitive && CheckInStockAvailability;

		private bool containsDateBirthPeriod;
		[PropertyChangedAlso(nameof(SensitiveDateBirth))]
		public bool ContainsDateBirthPeriod {
			get => containsDateBirthPeriod;
			set {
				if (value) ContainsPeriod = false;
				SetField(ref containsDateBirthPeriod, value);
			}
		}

		private DateTime startDateBirth;
		public DateTime StartDateBirth {
			get => startDateBirth;
			set => SetField(ref startDateBirth, value);
		}
		
		private DateTime endDateBirth;
		public DateTime EndDateBirth {
			get => endDateBirth;
			set => SetField(ref endDateBirth, value);
		}

		public int[] SelectedProtectionToolsIds => ChoiceProtectionToolsViewModel.SelectedIds;
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public ChoiceListViewModel<ProtectionTools> ChoiceProtectionToolsViewModel;
		#endregion

		public EmployeeNotificationFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<EmployeeNotificationFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
				.MakeByType()
				.Finish();
			startDateIssue = startDateBirth = DateTime.Today;
			endDateIssue = endDateBirth = startDateIssue.AddDays(14);
			Warehouses = new List<Warehouse>() { new Warehouse() { Id = -1, Name = "Все" } };
			Warehouses.AddRange(UoW.GetAll<Warehouse>());
			SelectedWarehouse = Warehouses[0];
			
			var protectionToolsList = UoW.GetAll<ProtectionTools>().ToList();
			ChoiceProtectionToolsViewModel = new ChoiceListViewModel<ProtectionTools>(protectionToolsList);
			ChoiceProtectionToolsViewModel.Items.PropertyOfElementChanged += (s,e)
				=> OnPropertyChanged(nameof(SelectedProtectionToolsIds));
		}
	}
	
	public enum AskIssueType
	{
		[Display(Name = "Все")]
		All,
		[Display(Name = "Персональная")]
		Personal,
		[Display(Name = "Коллективная")]
		Collective
	}

	public enum SexType 
	{
		[Display(Name="Все")]
		All,
		[Display(Name="Мужской")]
		M,
		[Display(Name="Женский")]
		F
	}
}
