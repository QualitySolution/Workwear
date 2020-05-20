using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.ViewModels;
using QSOrmProject;
using workwear.Dialogs.Organization;
using workwear.Domain.Company;
using workwear.Representations.Organization;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeVacationsViewModel : ViewModelBase
	{
		private readonly IDeleteEntityService deleteService;
		private readonly ITdiCompatibilityNavigation navigation;
		private readonly EmployeeViewModel employeeViewModel;
		private EmployeeVacationsVM employeeVacationsVM;

		public EmployeeVacationsViewModel(IDeleteEntityService deleteService, ITdiCompatibilityNavigation navigation, EmployeeViewModel employeeViewModel)
		{
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		#endregion

		#region Показ
		private bool isConfigured = false;

		public void OnShow()
		{
			if(!isConfigured) {
				isConfigured = true;
				EmployeeVacationsVM = new EmployeeVacationsVM(UoW) {
					Employee = Entity
				};
			}
		}

		public void UpdateList()
		{
			EmployeeVacationsVM.UpdateNodes();
		}

		#endregion

		#region Свойства

		public EmployeeVacationsVM EmployeeVacationsVM { get => employeeVacationsVM; private set => SetField(ref employeeVacationsVM, value); }

		#endregion

		#region Методы View

		public void AddItem()
		{
			if(UoW.IsNew) {
				if(CommonDialogs.SaveBeforeCreateSlaveEntity(typeof(EmployeeCard), typeof(EmployeeVacation))) {
					UoW.Save();
				}
				else
					return;
			}
			navigation.OpenTdiTab<EmployeeVacationDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void EditItem(int id)
		{
			navigation.OpenTdiTab<EmployeeVacationDlg, int>(employeeViewModel, id);
		}

		public void DeleteItem(int id)
		{
			deleteService.DeleteEntity<EmployeeVacation>(id);
		}

		public void DoubleClick(int id)
		{
			EditItem(id);
		}

		#endregion
	}
}
