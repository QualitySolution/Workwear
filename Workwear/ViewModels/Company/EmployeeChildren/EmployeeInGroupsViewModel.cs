using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels;
using Workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeInGroupsViewModel: ViewModelBase {
		
		private readonly INavigationManager navigation;
		private readonly EmployeeViewModel employeeViewModel;
		
		public EmployeeCard Entity => employeeViewModel.Entity;
		private IUnitOfWork UoW => employeeViewModel.UoW;
		private bool isConfigured = false;
		
		public EmployeeInGroupsViewModel(INavigationManager navigation, EmployeeViewModel employeeViewModel)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
		}

		public void OnShow() {
			if(!isConfigured) {
				isConfigured = true;
				OnPropertyChanged(nameof(EmployeeGroupItems));
			}
		}

		#region  Свойства View
		public IObservableList<EmployeeGroupItem> EmployeeGroupItems => Entity.EmployeeGroupItems;
		#endregion

		#region Методы

		public void AddGroups() {
			var selectJournal = navigation.OpenViewModel<EmployeeGroupJournalViewModel>(employeeViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddEmployeeGroups;
		}

		private void AddEmployeeGroups(object sender, JournalSelectedEventArgs e) {
			foreach(var group in UoW.GetById<EmployeeGroup>(e.SelectedObjects.Select(x => x.GetId())))
				Entity.AddEmployeeGroup(group);
		}

		public void DeleteItems(params EmployeeGroupItem[] deleteGroupItems) {
			foreach(var item in deleteGroupItems) {
				Entity.EmployeeGroupItems.Remove(item);
				item.Group.Items.Remove(item);
			}
		}
		#endregion
	}
}
