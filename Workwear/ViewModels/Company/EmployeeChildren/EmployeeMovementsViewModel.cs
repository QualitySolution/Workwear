using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Utilities.Debug;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Regulations;
using Workwear.Models.Operations;
using workwear.Models.Stock;
using Workwear.Repository.Operations;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Operations;

namespace Workwear.ViewModels.Company.EmployeeChildren
{
	public class EmployeeMovementsViewModel : ViewModelBase, IDisposable
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly EmployeeViewModel employeeViewModel;
		private readonly OpenStockDocumentsModel openStockDocumentsModel;
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly FeaturesService featuresService;
		private readonly EmployeeIssueModel issueModel;
		private readonly BaseParameters baseParameters;
		private readonly ITdiCompatibilityNavigation navigation;
		private readonly IInteractiveService interactive;
		List<EmployeeMovementItem> movements;

		public EmployeeMovementsViewModel(EmployeeViewModel employeeViewModel, 
			OpenStockDocumentsModel openStockDocumentsModel,  
			EmployeeIssueRepository employeeIssueRepository,  
			FeaturesService featuresService, 
			EmployeeIssueModel issueModel,
			BaseParameters baseParameters,
			
			IInteractiveService interactive,
			ITdiCompatibilityNavigation navigation)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.openStockDocumentsModel = openStockDocumentsModel ?? throw new ArgumentNullException(nameof(openStockDocumentsModel));
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));

			this.employeeIssueRepository.RepoUow = UoW;
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		public List<EmployeeMovementItem> Movements {
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
		
		public void RecalculateIssue(EmployeeMovementItem item) {
			issueModel.RecalculateDateOfIssue(new List<EmployeeIssueOperation>() {item.Operation}, baseParameters, interactive);
		}

		public void OpenDoc(EmployeeMovementItem item) {
			var cardItem = Entity.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == item.Operation.ProtectionTools);
			if(item.Operation.ManualOperation) {
				var page = navigation.OpenViewModel<ManualEmployeeIssueOperationsViewModel, EmployeeCardItem, EmployeeIssueOperation>(
					employeeViewModel, cardItem, item.Operation, OpenPageOptions.AsSlave);
				page.ViewModel.SaveChanged += SetIssueDateManual_PageClosed;
			}
			
			if(item.EmployeeIssueReference?.DocumentType == null)
				return;

			openStockDocumentsModel.EditDocumentDialog(employeeViewModel, item.EmployeeIssueReference);
		}

		public void RemoveOperation(EmployeeMovementItem item)
		{
			if(item.EmployeeIssueReference?.DocumentType != null)
				return;

			UoW.Delete(item.Operation);
			UpdateMovements();
			employeeViewModel.Save();
		}
		
		
		#region Замена Номенклатуры нормы
		
		public List<ProtectionTools> ProtectionToolsForChange => Entity.WorkwearItems.Select(x => x.ProtectionTools).ToList();

		public void ChangeProtectionTools(EmployeeMovementItem item, ProtectionTools protectionTools) {
			List<ProtectionTools> protectionToolsForUpdate = new List<ProtectionTools> { protectionTools };
			if (item.Operation.ProtectionTools != null) 
				protectionToolsForUpdate.Add(item.Operation.ProtectionTools);

			item.Operation.ProtectionTools = protectionTools;
			if(item.Operation.Issued > 0)
				item.Operation.NormItem = Entity.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools)?.ActiveNormItem;
			UoW.Save(item.Operation);
			
			if(item.EmployeeIssueReference?.DocumentType == null)
				return;
			if(item.EmployeeIssueReference.ItemId == null)
				throw new NullReferenceException("ItemID is Null");
			
			switch(item.EmployeeIssueReference.DocumentType) {
				case StockDocumentType.ExpenseEmployeeDoc:
					var docI =  UoW.GetById<ExpenseItem>(item.EmployeeIssueReference.ItemId.Value);
					docI.ProtectionTools = protectionTools;
					UoW.Save(docI);
					break;
				case StockDocumentType.CollectiveExpense:
					var docC =  UoW.GetById<CollectiveExpenseItem>(item.EmployeeIssueReference.ItemId.Value);
					docC.ProtectionTools = protectionTools;
					UoW.Save(docC);
					break;
				case StockDocumentType.IncomeDoc:
				case StockDocumentType.InspectionDoc:
				case StockDocumentType.WriteoffDoc:
					//Ничего не делаем, так как в этих типах документов нет номенклатуры нормы, она только в операции.
					break;
				default:
					throw new NotSupportedException("Unknown document type.");
			}

			Entity.FillWearReceivedInfo(employeeIssueRepository);
			Entity.UpdateNextIssue(protectionToolsForUpdate.ToArray());
		}

		public void OpenJournalChangeProtectionTools(EmployeeMovementItem item) {
			var selectJournal = navigation.OpenViewModel<ProtectionToolsJournalViewModel>(employeeViewModel, QS.Navigation.OpenPageOptions.AsSlave);
			
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += ChangeProtectionToolsFromJournal;
		}
		private void ChangeProtectionToolsFromJournal(object sender, JournalSelectedEventArgs e) {
			var page = navigation.FindPage((DialogViewModelBase)sender);
			var item = page.Tag as EmployeeMovementItem;
			ChangeProtectionTools(item, UoW.GetById<ProtectionTools>(e.SelectedObjects.First().GetId()));
		}

		public void MakeEmptyProtectionTools(EmployeeMovementItem item) {
			if(item.EmployeeIssueReference?.DocumentType != null) {
				switch(item.EmployeeIssueReference.DocumentType) {
					case StockDocumentType.ExpenseEmployeeDoc:
						var docPerItem =  UoW.GetById<ExpenseItem>(item.EmployeeIssueReference.ItemId.Value);
						docPerItem.ProtectionTools = null;
						UoW.Save(docPerItem);
						break;
					case StockDocumentType.CollectiveExpense:
						var docColItem =  UoW.GetById<CollectiveExpenseItem>(item.EmployeeIssueReference.ItemId.Value);
						docColItem.ProtectionTools = null;
						UoW.Save(docColItem);
						break;
					case StockDocumentType.IncomeDoc:
					case StockDocumentType.InspectionDoc:
					case StockDocumentType.WriteoffDoc:
						//Ничего не делаем, так как в этих типах документов нет номенклатуры нормы, она только в операции.
						break;
					default:
						throw new NotSupportedException("Unknown document type.");
				}
			}
			item.Operation.ProtectionTools = null;
			UoW.Save(item.Operation);
		}

		#endregion
		void SetIssueDateManual_PageClosed(ProtectionTools protectionTools) {
			UoW.Commit();
			Entity.FillWearReceivedInfo(employeeIssueRepository);
			Entity.UpdateNextIssue(protectionTools);
		}

		#endregion

		#region Внутренние

		private void UpdateMovements()
		{
			logger.Info("Обновляем историю выдачи...");
			var performance = new PerformanceHelper(logger: logger);
			var prepareMovements = new List<EmployeeMovementItem>();

			var list = employeeIssueRepository.AllOperationsForEmployee(Entity, query => query
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature.Type)
				.Fetch(SelectMode.Fetch, x => x.ProtectionTools)
				.Fetch(SelectMode.Fetch, x => x.ProtectionTools.Type)
				.Fetch(SelectMode.Fetch, x => x.WarehouseOperation)
				.Fetch(SelectMode.Skip, x => x.BarcodeOperations)
			);
			performance.CheckPoint("Получение операций");
			var docs = employeeIssueRepository.GetReferencedDocuments(list.Select(x => x.Id).ToArray());
			performance.CheckPoint("Получение ссылок на документы");
			foreach(var operation in list) {
				var item = new EmployeeMovementItem();
				item.Operation = operation;
				item.EmployeeIssueReference = docs.FirstOrDefault(x => x.OperationId == operation.Id);
				item.PropertyChanged += Item_PropertyChanged;
				prepareMovements.Add(item);
			}
			Movements = prepareMovements;

			performance.CheckPoint("Подготовка данных");
			performance.PrintAllPoints(logger);
			logger.Info($"Обновили за {performance.TotalTime.TotalSeconds} сек.");
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == PropertyUtil.GetName<EmployeeMovementItem>(x => x.UseAutoWriteOff)) {
				var item = sender as EmployeeMovementItem;
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
