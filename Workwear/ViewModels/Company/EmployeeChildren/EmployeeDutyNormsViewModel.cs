using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Models.Operations;
using Workwear.Repository.Regulations;
using Workwear.Tools.Features;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeDutyNormsViewModel: ViewModelBase {
		private readonly EmployeeViewModel employeeViewModel;
		private readonly INavigationManager navigation;
		private IUnitOfWork UoW => employeeViewModel.UoW;
		private readonly DutyNormIssueModel dutyNormIssueModel;
		public readonly DutyNormRepository dutyNormRepository;
		private readonly FeaturesService featuresService;
		private bool isConfigured = false;
		
		public EmployeeDutyNormsViewModel(
			EmployeeViewModel employeeViewModel,
			INavigationManager navigation,
			FeaturesService featuresService,
			DutyNormIssueModel dutyNormIssueModel,
			DutyNormRepository dutyNormRepository
			) 
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.dutyNormIssueModel = dutyNormIssueModel ?? throw new ArgumentNullException(nameof(dutyNormIssueModel));
			this.dutyNormRepository = dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
		}
		
		public void OnShow() {
			if(!isConfigured) {
				DutyNormsItemsList = new ObservableList<DutyNormItem>(dutyNormRepository.AllItemsFor(responsibleemployees: new [] {Entity}));
				dutyNormIssueModel.FillDutyNormItems(DutyNormsItemsList.ToArray());
				isConfigured = true;
			}
		}
		
	    #region Свойства

		private IObservableList<DutyNormItem> dutyNormsItemsList = new ObservableList<DutyNormItem>();
		public IObservableList<DutyNormItem> DutyNormsItemsList {
			get => dutyNormsItemsList;
			set => SetField(ref dutyNormsItemsList, value);
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
