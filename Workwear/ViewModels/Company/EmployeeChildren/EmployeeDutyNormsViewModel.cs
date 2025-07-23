using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
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
		private readonly DutyNormIssueModel dutyNormIssueModel;
		private readonly DutyNormRepository dutyNormRepository;
		private readonly FeaturesService featuresService;
		private bool isConfigured = false;
		
		public EmployeeDutyNormsViewModel(
			EmployeeViewModel employeeViewModel,
			FeaturesService featuresService,
			DutyNormIssueModel dutyNormIssueModel,
			DutyNormRepository dutyNormRepository,
			INavigationManager navigation,
            IEntityChangeWatcher changeWatcher
			) 
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.dutyNormIssueModel = dutyNormIssueModel ?? throw new ArgumentNullException(nameof(dutyNormIssueModel));
			this.dutyNormRepository = dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
			if(changeWatcher == null) throw new ArgumentNullException(nameof(changeWatcher));
			
			//Для синхронизации с изменениями внесёнными в базу. Например, создании документов выдачи
			foreach(var dutyNorm in Entity.RelatedDutyNorms) {
				changeWatcher.BatchSubscribe(DutyNormChangeEvent)
					.IfEntity<DutyNormIssueOperation>()
					.AndWhere(op => op.DutyNorm.Id == dutyNorm.Id);
			}
		}
		
		public void OnShow() {
			if(!isConfigured) {
				Load();
				isConfigured = true;
			}
		}

		private void Load() {
			DutyNormsItemsList = new ObservableList<DutyNormItem>(dutyNormRepository.AllItemsFor(responsibleemployeesIds: new[] { Entity.Id }));
			dutyNormRepository.LoadFullInfo(Entity.RelatedDutyNorms.Select(x => x.Id).ToArray());
			dutyNormIssueModel.FillDutyNormItems(DutyNormsItemsList.ToArray());
		}

		private void DutyNormChangeEvent(EntityChangeEvent[] changeevents) {
			foreach(var changeEvent in changeevents) {
				if(changeEvent.EventType != TypeOfChangeEvent.Insert) {
					var op = UoW.GetById<DutyNormIssueOperation>(changeEvent.Entity.GetId());
					if(op != null)
						UoW.Session.Evict(op);
				}
				Load();
			}
		}
		
	    #region Свойства и пробросы
	    
	    private EmployeeCard Entity => employeeViewModel.Entity;
	    private IUnitOfWork UoW => employeeViewModel.UoW;
	    
		private IObservableList<DutyNormItem> dutyNormsItemsList = new ObservableList<DutyNormItem>();
		public IObservableList<DutyNormItem> DutyNormsItemsList {
			get => dutyNormsItemsList;
			set => SetField(ref dutyNormsItemsList, value);
		}

		#endregion
		
		#region Для View

		public void GiveWearByDutyNorm(DutyNormItem dutyNormItem) => navigation.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder, DutyNorm>
			(employeeViewModel, EntityUoWBuilder.ForCreate(), dutyNormItem.DutyNorm);
		
		public void OpenDutyNorm(DutyNormItem dutyNormItem) => navigation.OpenViewModel<DutyNormViewModel, IEntityUoWBuilder>(employeeViewModel,
				EntityUoWBuilder.ForOpen(dutyNormItem.DutyNorm.Id));
		
		#endregion
		
	}
}
