using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.DTO;
using workwear.Models.Stock;
using workwear.Repository.Operations;
using workwear.Tools.Features;
using workwear.ViewModels.Operations;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeMovementsViewModel : ViewModelBase, IDisposable
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly EmployeeViewModel employeeViewModel;
		private readonly OpenStockDocumentsModel openStockDocumentsModel;
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly FeaturesService featuresService;
		private readonly ITdiCompatibilityNavigation navigation;
		List<EmployeeCardMovements> movements;

		public EmployeeMovementsViewModel(EmployeeViewModel employeeViewModel, OpenStockDocumentsModel openStockDocumentsModel,  EmployeeIssueRepository employeeIssueRepository,  FeaturesService featuresService, ITdiCompatibilityNavigation navigation)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.openStockDocumentsModel = openStockDocumentsModel ?? throw new ArgumentNullException(nameof(openStockDocumentsModel));
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

			this.employeeIssueRepository.RepoUow = UoW;
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		public List<EmployeeCardMovements> Movements {
			get => movements; set => SetField(ref movements, value);
		}

		#endregion
		#region Visible
		public bool VisibleSignColumn => featuresService.Available(WorkwearFeature.IdentityCards);
		#endregion
		#region Публичные
		private bool isConfigured = false;

		public void OnShow()
		{
			if(!isConfigured) {
				isConfigured = true;
				NotifyConfiguration.Instance.BatchSubscribeOnEntity<EmployeeIssueOperation>(HandleManyEntityChangeEventMethod);
				UpdateMovements();
			}
		}
		#endregion
		#region Контекстное меню
		public void OpenDoc(EmployeeCardMovements item) {
			var cardItem = Entity.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == item.Operation.ProtectionTools);
			if(item.Operation.OverrideBefore) {
				var page = navigation.OpenViewModel<ManualEmployeeIssueOperationsViewModel, EmployeeCardItem, EmployeeIssueOperation>(
					employeeViewModel, cardItem, item.Operation, OpenPageOptions.AsSlave);
				page.ViewModel.SaveChanged += SetIssueDateManual_PageClosed;
			}
			
			if(item.EmployeeIssueReference?.DocumentType == null)
				return;

			openStockDocumentsModel.EditDocumentDialog(employeeViewModel, item.EmployeeIssueReference);
		}

		public void RemoveOperation(EmployeeCardMovements item)
		{
			if(item.EmployeeIssueReference?.DocumentType != null)
				return;

			UoW.Delete(item.Operation);
			UpdateMovements();
			employeeViewModel.Save();
		}
		
		void SetIssueDateManual_PageClosed(ProtectionTools protectionTools) {
			UoW.Commit();
			Entity.FillWearRecivedInfo(employeeIssueRepository);
			Entity.UpdateNextIssue(protectionTools);
		}

		#endregion

		#region Внутренние

		private void UpdateMovements()
		{
			logger.Info("Обновляем историю выдачи...");

			var prepareMovements = new List<EmployeeCardMovements>();

			var list = employeeIssueRepository.AllOperationsForEmployee(Entity, query => query.Fetch(SelectMode.Fetch, x => x.Nomenclature));
			var docs = employeeIssueRepository.GetReferencedDocuments(list.Select(x => x.Id).ToArray());
			foreach(var operation in list) {
				var item = new EmployeeCardMovements();
				item.Operation = operation;
				item.EmployeeIssueReference = docs.FirstOrDefault(x => x.OperationId == operation.Id);
				item.PropertyChanged += Item_PropertyChanged;
				prepareMovements.Add(item);
			}
			Movements = prepareMovements;

			logger.Info("Ок");
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == PropertyUtil.GetName<EmployeeCardMovements>(x => x.UseAutoWriteOff)) {
				var item = sender as EmployeeCardMovements;
				UoW.Save(item.Operation);
			}
		}

		#endregion

		void HandleManyEntityChangeEventMethod(EntityChangeEvent[] changeEvents)
		{
			var updatedOperations = changeEvents.Where(x => x.GetEntity<EmployeeIssueOperation>().Employee.IsSame(employeeViewModel.Entity)).ToList();
			if(updatedOperations.Count > 0) {
				Movements.ForEach(m => UoW.Session.Evict(m.Operation));
				UpdateMovements();
			}
		}

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}
