using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate.Classic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels;
using Workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeCostCentersViewModel : ViewModelBase{
		
		private readonly IDeleteEntityService deleteService;
		private readonly ITdiCompatibilityNavigation navigation;
		private readonly EmployeeViewModel employeeViewModel;
		
		public EmployeeCard Entity => employeeViewModel.Entity;
		private IUnitOfWork UoW => employeeViewModel.UoW;
		
		public EmployeeCostCentersViewModel(IDeleteEntityService deleteService, ITdiCompatibilityNavigation navigation, EmployeeViewModel employeeViewModel)
		{
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
		}

		#region Методы
		public void AddItem() {
			var selectJournal = navigation.OpenViewModel<CostCenterJournalViewModel>(employeeViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddEmployeeCostCenter;
		}

		private void AddEmployeeCostCenter(object sender, JournalSelectedEventArgs e) {
			int costCenterCount = Entity.CostCenters.Count;
			decimal sumPercent = Entity.CostCenters.Sum(x => x.Percent);
			decimal addPercent = Math.Round((1m - sumPercent) / e.SelectedObjects.Length, 2);
			UoW.GetById<CostCenter>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddCostCenter(new EmployeeCostCenter(Entity, n, addPercent)));
			
			if(costCenterCount < Entity.CostCenters.Count)
				Entity.CostCenters.Last().Percent -= Entity.CostCenters.Sum(x => x.Percent) - 1m; //  подгоняем сумму под 1
		}

		public void DeleteItem(EmployeeCostCenter deleteItem) {
			if(deleteItem.Id > 0) {
				UoW.Delete(deleteItem);
			}
			Entity.ObservableCostCenters.Remove(deleteItem);
		}
		#endregion

		public string Validate() {
			decimal sumPercent = Entity.CostCenters.Sum(x => x.Percent);
			if(sumPercent != 1)
				return "Сумма по МВЗ в должна быть равна 100";
			else return null;
		}
	}
}
