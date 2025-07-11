using System;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Models.Operations;
using Workwear.Tools.Features;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeDutyNormsViewModel: ViewModelBase {
		private readonly EmployeeViewModel employeeViewModel;
		private readonly INavigationManager navigation;
		private IUnitOfWork UoW => employeeViewModel.UoW;
		private readonly DutyNormIssueModel dutyNormIssueModel;
		private readonly FeaturesService featuresService;
		private bool isConfigured = false;
		
		public EmployeeDutyNormsViewModel(
			EmployeeViewModel employeeViewModel,
			INavigationManager navigation,
			FeaturesService featuresService,
			DutyNormIssueModel dutyNormIssueModel
			) 
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.dutyNormIssueModel = dutyNormIssueModel ?? throw new ArgumentNullException(nameof(dutyNormIssueModel));
		}
		
		public void OnShow() {
			if(!isConfigured) {
				AllDutyNormsItemsForResponsibleEmployee = dutyNormIssueModel.GetAllDutyNormsItemsForResponsibleEmployee(Entity);
				isConfigured = true;
			}
		}
		
	    #region Свойства

		private IObservableList<DutyNormItem> allDutyNormsItemsForResponsibleEmployee = new ObservableList<DutyNormItem>();
		public IObservableList<DutyNormItem> AllDutyNormsItemsForResponsibleEmployee {
			get => allDutyNormsItemsForResponsibleEmployee;
			set => SetField(ref allDutyNormsItemsForResponsibleEmployee, value);
		}

		#endregion
		
		#region Хелперы
		private EmployeeCard Entity => employeeViewModel.Entity;
		#endregion
		#region Действия View

		public void GiveWearByDutyNorm(DutyNormItem dutyNormItem) => navigation.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder, DutyNorm>
			(employeeViewModel, EntityUoWBuilder.ForCreate(), dutyNormItem.DutyNorm);
		
		public void OpenDutyNorm(DutyNormItem dutyNormItem) => navigation.OpenViewModel<DutyNormViewModel, IEntityUoWBuilder>(employeeViewModel,
				EntityUoWBuilder.ForOpen(dutyNormItem.DutyNorm.Id));
		
		#endregion
		
	}
}
