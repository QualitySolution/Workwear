using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Repository.Regulations;
using Workwear.Tools.Features;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeDutyNormsViewModel: ViewModelBase {
		private readonly EmployeeViewModel employeeViewModel;
		private readonly INavigationManager navigation;
		private DutyNormRepository dutyNormRepository;
		private IUnitOfWork UoW => employeeViewModel.UoW;
		private readonly FeaturesService featuresService;
		
		public EmployeeDutyNormsViewModel(
			EmployeeViewModel employeeViewModel,
			INavigationManager navigation,
			FeaturesService featuresService,
			DutyNormRepository dutyNormRepository
			) 
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.dutyNormRepository = dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
			if(featuresService.Available(WorkwearFeature.DutyNorms))
				AllDutyNormsItemsForResponsibleEmployee.AddRange(dutyNormRepository.GetAllDutyNormsItemsForResponsibleEmployee(Entity));
		}

		#region Хелперы
		private EmployeeCard Entity => employeeViewModel.Entity;
		#endregion

		private List<DutyNormItem> allDutyNormsItemsForResponsibleEmployee = new List<DutyNormItem>();

		public List<DutyNormItem> AllDutyNormsItemsForResponsibleEmployee {
			get => allDutyNormsItemsForResponsibleEmployee;
			set {
				SetField(ref allDutyNormsItemsForResponsibleEmployee, value);
				OnPropertyChanged();
			}
		}
		
		#region Действия View

		public void GiveWearByDutyNorm(DutyNormItem dutyNormItem) => navigation.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder, DutyNorm>
			(employeeViewModel, EntityUoWBuilder.ForCreate(), dutyNormItem.DutyNorm);
		
		public void OpenDutyNorm(DutyNormItem dutyNormItem) => navigation.OpenViewModel<DutyNormViewModel, IEntityUoWBuilder>(employeeViewModel,
				EntityUoWBuilder.ForOpen(dutyNormItem.DutyNorm.Id));
		
		#endregion
		
	}
}
