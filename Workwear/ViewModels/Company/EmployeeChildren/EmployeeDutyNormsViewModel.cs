using System;
using System.ComponentModel;
using NHibernate;
using NPOI.SS.Formula.Functions;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Models.Operations;
using Workwear.Repository.Regulations;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeDutyNormsViewModel: ViewModelBase {
		private readonly EmployeeViewModel employeeViewModel;
		private readonly INavigationManager navigation;
		private IUnitOfWork UoW => employeeViewModel.UoW;
		public IObservableList<DutyNormItem> ObservableDutyNormItems = new ObservableList<DutyNormItem>();

		public EmployeeDutyNormsViewModel(
			EmployeeViewModel employeeViewModel,
			INavigationManager navigation
			) 
		{
			DutyNorm dutyNormAlias = null;
			DutyNormItem dutyNormItemAlias = null;
			EmployeeCard employeeCardAlias = null;
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			var query = UoW.Session.QueryOver<DutyNormItem>(() => dutyNormItemAlias)
				.JoinEntityAlias(() => dutyNormAlias, (() => dutyNormItemAlias.DutyNorm.Id == dutyNormAlias.Id))
				.JoinEntityAlias(() => employeeCardAlias, (() => dutyNormAlias.ResponsibleEmployee.Id == employeeCardAlias.Id))
				.Where(() => employeeCardAlias.Id == Entity.Id);
			foreach(var item in query.List())
				ObservableDutyNormItems.Add(item);
			foreach(var item in ObservableDutyNormItems) {
				item.Update(UoW);
			}
		}

		#region Хелперы

		private EmployeeCard Entity => employeeViewModel.Entity;
		#region Показ
		public bool IsConfigured {get; private set; }

		public void OnShow() {
			if (IsConfigured) return;
			IsConfigured = true;
			OnPropertyChanged(nameof(ObservableDutyNormItems));
		}
		#endregion
		#endregion

		#region Свойства

		
		private DutyNormItem selectedItem;
		public virtual DutyNormItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}


		#endregion
		
		#region Действия View

		public void GiveWearByDutyNorm() 
		{
			navigation.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder>(employeeViewModel, EntityUoWBuilder.ForCreate());
		}

		public void OpenDutyNorm(DutyNormItem dutyNormItem) => navigation.OpenViewModel<DutyNormViewModel, IEntityUoWBuilder>(employeeViewModel,
				EntityUoWBuilder.ForOpen(dutyNormItem.DutyNorm.Id));
		
		#endregion

		#region Обработка изменений
		
		#endregion
		
	}
}
