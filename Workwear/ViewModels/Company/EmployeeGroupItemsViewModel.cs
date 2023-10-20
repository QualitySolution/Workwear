using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;


namespace Workwear.ViewModels.Company {
	public class EmployeeGroupItemsViewModel : ViewModelBase {
		public EmployeeGroupItemsViewModel(
			EmployeeGroupViewModel parent,
			INavigationManager navigation) {
			this.parent = parent;
			this.navigation = navigation;

			if(parent.Entity.Id == 0)
				this.parent.CurrentTab = 0;
		}

		private readonly EmployeeGroupViewModel parent;
		private readonly INavigationManager navigation;
		
		public IObservableList<EmployeeGroupItem> Items => parent.Entity.Items;

		#region Действия View

		public void AddEmployees() {
			var selectJournal = navigation.OpenViewModel<EmployeeJournalViewModel>(parent, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadEmployees;
		}
		void LoadEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var selectedIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id);
			parent.Entity.Add(parent.UoW.GetById<EmployeeCard>(selectedIds));
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
