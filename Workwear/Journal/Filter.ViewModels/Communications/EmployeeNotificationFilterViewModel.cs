using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.ReportParameters.ViewModels;

namespace workwear.Journal.Filter.ViewModels.Communications
{
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
		public bool ContainsPeriod {
			get => containsPeriod;
			set {
				if (value) ContainsDateBirthPeriod = false;
				SetField(ref containsPeriod, value);
			}
		}

		private bool checkInInStockAvailability;
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

		public List<int> SelectedProtectionToolsIds { get; set; }
		#endregion

		#region ViewModels
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public ChoiceProtectionToolsViewModel ChoiceProtectionToolsViewModel;
		#endregion

		public EmployeeNotificationFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<EmployeeNotificationFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
				.MakeByType()
				.Finish();
			startDateIssue = startDateBirth = DateTime.Today;
			endDateIssue = endDateBirth = startDateIssue.AddDays(14);
			Warehouses = UoW.GetAll<Warehouse>().ToList();

			ChoiceProtectionToolsViewModel = new ChoiceProtectionToolsViewModel(UoW);
			ChoiceProtectionToolsViewModel.PropertyChanged += ChoiceViewModelOnPropertyChanged;
			SelectedProtectionToolsIds = new List<int>(ChoiceProtectionToolsViewModel.SelectedIds);
		}
		
		private void ChoiceViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) 
		{
			if (SelectedProtectionToolsIds.SequenceEqual(ChoiceProtectionToolsViewModel.SelectedIds)) 
			{
				return;
			}
			
			int val = ChoiceProtectionToolsViewModel.SelectedIdsMod.FirstOrDefault();
			if (val == -1) 
			{
				SelectedProtectionToolsIds.Clear();
				SelectedProtectionToolsIds.AddRange(ChoiceProtectionToolsViewModel.SelectedIds);
				OnPropertyChanged(nameof(SelectedProtectionToolsIds));
				return;
			}
			else if (val == -2) 
			{
				SelectedProtectionToolsIds.Clear();
				OnPropertyChanged(nameof(SelectedProtectionToolsIds));
				return;
			}
			
			List<int> newIds = ChoiceProtectionToolsViewModel.SelectedIds.Except(SelectedProtectionToolsIds).ToList();
			if (newIds.Any()) 
			{
				newIds.ForEach(id => SelectedProtectionToolsIds.Add(id));
			}

			List<int> exceptIds = SelectedProtectionToolsIds.Except(ChoiceProtectionToolsViewModel.SelectedIds).ToList();
			if (exceptIds.Any()) 
			{
				exceptIds.ForEach(id => SelectedProtectionToolsIds.Remove(id));
			}

			OnPropertyChanged(nameof(SelectedProtectionToolsIds));
		}
	}
	
	public enum AskIssueType
	{
		[Display(Name = "Все")]
		All,
		[Display(Name = "Персональная")]
		Personal,
		[Display(Name = "Коллективная")]
		Сollective
	}
}
