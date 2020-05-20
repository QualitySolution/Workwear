using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using workwear.Domain.Company;
using workwear.Representations.Organization;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeListedItemsViewModel : ViewModelBase
	{
		public EmployeeListedItemsViewModel(EmployeeViewModel employeeViewModel, ITdiCompatibilityNavigation navigation)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		#endregion

		#region Показ
		private bool isConfigured = false;
		private readonly EmployeeViewModel employeeViewModel;
		private readonly ITdiCompatibilityNavigation navigation;

		public void OnShow()
		{
			if(!isConfigured) {
				isConfigured = true;
				EmployeeBalanceVM = new EmployeeBalanceVM(UoW) {
					Employee = Entity
				};
			}
		}

		public void UpdateList()
		{
			EmployeeBalanceVM.UpdateNodes();
		}

		#endregion

		#region Свойства

		private EmployeeBalanceVM employeeBalanceVM;
		public EmployeeBalanceVM EmployeeBalanceVM { get => employeeBalanceVM; private set => SetField(ref employeeBalanceVM, value); }

		public bool SensetiveButtonGiveWear => !employeeViewModel.UoW.IsNew;
		public bool SensetiveButtonReturn => !employeeViewModel.UoW.IsNew;
		public bool SensetiveButtonWriteoff => !employeeViewModel.UoW.IsNew;

		#endregion

		#region Действия View

		public void GiveWear()
		{
			if(!employeeViewModel.Save())
				return;

			navigation.OpenTdiTab<ExpenseDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void ReturnWear()
		{
			navigation.OpenTdiTab<IncomeDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void WriteOffWear()
		{
			navigation.OpenTdiTab<WriteOffDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		#endregion

	}
}
