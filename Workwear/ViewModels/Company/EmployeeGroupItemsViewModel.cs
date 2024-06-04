using System.Linq;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;


namespace Workwear.ViewModels.Company {
	public class EmployeeGroupItemsViewModel : ViewModelBase {
		public EmployeeGroupItemsViewModel(
			EmployeeGroupViewModel employeeGroupViewModel,
			INavigationManager navigation) {
			this.employeeGroupViewModel = employeeGroupViewModel;
			this.navigation = navigation;

			if(employeeGroupViewModel.Entity.Id == 0)
				this.employeeGroupViewModel.CurrentTab = 0;
		}

		private readonly EmployeeGroupViewModel employeeGroupViewModel;
		private readonly INavigationManager navigation;
		
		public IObservableList<EmployeeGroupItem> Items => employeeGroupViewModel.Entity.Items;

		#region Действия View

		public void AddEmployees() {
			var selectJournal = navigation.OpenViewModel<EmployeeJournalViewModel>(employeeGroupViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadEmployees;
		}
		void LoadEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var selectedIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id);
			employeeGroupViewModel.Entity.AddEmployees(employeeGroupViewModel.UoW.GetById<EmployeeCard>(selectedIds));
		}
		
		public void Remove(EmployeeGroupItem[] items) {
			foreach(var item in items) {
				Items.Remove(item);
			}
		}

		public void OpenEmployees(EmployeeGroupItem[] items) {
			foreach(var item in items)
				navigation.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(item.Employee.Id));
		}
		#endregion
	}
}
