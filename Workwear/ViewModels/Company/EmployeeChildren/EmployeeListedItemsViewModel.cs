﻿using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using workwear;
using Workwear.Domain.Company;
using workwear.Representations.Organization;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren
{
	public class EmployeeListedItemsViewModel : ViewModelBase
	{
		public EmployeeListedItemsViewModel(EmployeeViewModel employeeViewModel, ITdiCompatibilityNavigation navigation, FeaturesService featuresService)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.featuresService = featuresService;
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		#endregion

		#region Показ
		private bool isConfigured = false;
		private readonly EmployeeViewModel employeeViewModel;
		private readonly FeaturesService featuresService;
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
		public bool SensetiveButtonInspecton => !employeeViewModel.UoW.IsNew;
		public bool VisibleButtonInspecton => featuresService.Available(WorkwearFeature.Inspection);

		#endregion

		#region Действия View

		public void GiveWear()
		{
			if(!employeeViewModel.Save())
				return;

			navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void ReturnWear()
		{
			navigation.OpenTdiTab<IncomeDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void WriteOffWear()
		{
			navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void InspectionWear() {
			navigation.OpenViewModel<InspectionViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}
		#endregion

	}
}
