using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using workwear.Models.Stock;
using Workwear.Repository.Operations;
using Workwear.ViewModels.Operations;
using Workwear.ViewModels.Regulations;
using Workwear.Tools;
using Workwear.ViewModels.Stock;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Company.EmployeeChildren
{
	public class EmployeeWearItemsViewModel : ViewModelBase, IDisposable
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private readonly EmployeeViewModel employeeViewModel;
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly IInteractiveService interactive;
		private readonly INavigationManager navigation;
		private readonly ITdiCompatibilityNavigation tdiNavigation; //FIXME Временно пока не перепишем IncomeDocDlg на ViewModel
		private readonly OpenStockDocumentsModel stockDocumentsModel;
		private readonly IProgressBarDisplayable progress;

		public readonly BaseParameters BaseParameters;
		public EmployeeWearItemsViewModel(
			EmployeeViewModel employeeViewModel,
			EmployeeIssueRepository employeeIssueRepository,
			BaseParameters baseParameters,
			IInteractiveService interactive,
			INavigationManager navigation,
			ITdiCompatibilityNavigation tdiNavigation,
			OpenStockDocumentsModel stockDocumentsModel,
			FeaturesService featuresService,
			IProgressBarDisplayable progress)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.tdiNavigation = tdiNavigation ?? throw new ArgumentNullException(nameof(tdiNavigation));
			this.stockDocumentsModel = stockDocumentsModel ?? throw new ArgumentNullException(nameof(stockDocumentsModel));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<EmployeeCardItem, EmployeeIssueOperation>(HandleEntityChangeEvent);
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		#endregion

		#region Показ
		private bool isConfigured = false;

		public void OnShow()
		{
			if (isConfigured) return;
			isConfigured = true;
			var start = DateTime.Now;
			progress.Start(2+4);
			Entity.FillWearInStockInfo(UoW, BaseParameters, Entity.Subdivision?.Warehouse, DateTime.Now, progressStep: () => progress.Add());
			progress.Add();
			Entity.FillWearReceivedInfo(employeeIssueRepository);
			progress.Add();
			OnPropertyChanged(nameof(ObservableWorkwearItems));
			progress.Close();
			logger.Debug($"Время заполнения таблицы «Спецодежда по нормам»: {(DateTime.Now - start).TotalSeconds} сек." );
		}

		#endregion

		#region Свойства

		public GenericObservableList<EmployeeCardItem> ObservableWorkwearItems => Entity.ObservableWorkwearItems;

		public FeaturesService FeaturesService { get; }

		#endregion

		#region Внутренне
		void HandleEntityChangeEvent(EntityChangeEvent[] changeEvents)
		{
			if(!isConfigured)
				return;
			bool isMySession = changeEvents.First().Session == UoW.Session;
			//Не чего не делаем если это наше собственное изменение.
			if(!isMySession && changeEvents.Where(x => x.EventType == TypeOfChangeEvent.Delete)
				.Select(e => e.Entity).OfType<EmployeeCardItem>()
				.Any(x => x.EmployeeCard.IsSame(Entity))) {
				//Если сделано удаление строк, просто закрываем диалог,
				//так как заставить корректно сохранить сотрудника все равно не поучится.
				//Не работал следующий сценарий: Открываем диалог сотрудника,
				//строка добавленная по норме есть в списке, открываем норму, удаляем одну из строк, сохраняем норму.
				//После этого пытаемся сохранить сотрудника.
				var page = navigation.FindPage(employeeViewModel);
				navigation.ForceClosePage(page, CloseSource.Self);
				return;
			}

			foreach(var op in changeEvents
				        .Where(x => x.EventType == TypeOfChangeEvent.Update)
				        .Select(x => x.Entity)
				        .OfType<EmployeeIssueOperation>()
				        .Where(x => x.Employee.IsSame(Entity))) {
				
				var myOP = UoW.Session.Get<EmployeeIssueOperation>(op.Id);
				UoW.Session.Refresh(myOP);
			}

			if(!isMySession && changeEvents.Select(e => e.Entity).OfType<EmployeeCardItem>().Any(x => x.EmployeeCard.IsSame(Entity))) {
				RefreshWorkItems();
			}
		}

		#endregion

		#region Действия View

		public void GiveWearByNorm()
		{
			if(!employeeViewModel.Save())
				return;
			navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void ReturnWear()
		{
			tdiNavigation.OpenTdiTab<IncomeDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void OpenTimeLine(EmployeeCardItem item)
		{
			navigation.OpenViewModel<EmployeeIssueGraphViewModel, EmployeeCard, ProtectionTools>(employeeViewModel, Entity, item.ProtectionTools);
		}

		public void WriteOffWear()
		{
			navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void UpdateWorkwearItems()
		{
			Entity.UpdateWorkwearItems();
			Entity.FillWearInStockInfo(UoW, BaseParameters, Entity.Subdivision?.Warehouse, DateTime.Now);
			Entity.UpdateNextIssueAll();
		}

		public void SetIssueDateManual(EmployeeCardItem row)
		{
			var operations = employeeIssueRepository
				.GetOperationsForEmployee(Entity, row.ProtectionTools, UoW)
				.OrderByDescending(x => x.OperationTime)
				.ToList();
			IPage<ManualEmployeeIssueOperationsViewModel> page;
			if(!operations.Any() || operations.First().ExpiryByNorm < DateTime.Today)
				page = navigation.OpenViewModel<ManualEmployeeIssueOperationsViewModel, EmployeeCardItem>(
					employeeViewModel, row, OpenPageOptions.AsSlave);
			else if(operations.First().ManualOperation)
				page = navigation.OpenViewModel<ManualEmployeeIssueOperationsViewModel, EmployeeCardItem, EmployeeIssueOperation>(
					employeeViewModel, row, operations.First(), OpenPageOptions.AsSlave);
			else if(interactive.Question($"Для «{row.ProtectionTools.Name}» уже выполнялись полноценные выдачи, " +
			                             $"внесение ручных изменений может привести к нежелательным результатам. Продолжить?"))
				page = navigation.OpenViewModel<ManualEmployeeIssueOperationsViewModel, EmployeeCardItem>(
					employeeViewModel, row, OpenPageOptions.AsSlave);
			else
				return;
			page.ViewModel.SaveChanged += SetIssueDateManual_PageClosed;
		}

		void SetIssueDateManual_PageClosed(ProtectionTools protectionTools)
		{
			UoW.Commit();
			Entity.FillWearReceivedInfo(employeeIssueRepository);
			Entity.UpdateNextIssue(protectionTools);
			UoW.Save();
		}
		#endregion
		#region Контекстное меню
		public void OpenProtectionTools(EmployeeCardItem row)
		{
			navigation.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(employeeViewModel, EntityUoWBuilder.ForOpen(row.ProtectionTools.Id));
		}
		
		public void OpenActiveNorm(EmployeeCardItem row)
		{
			var page = navigation.OpenViewModel<NormViewModel, IEntityUoWBuilder>(employeeViewModel, EntityUoWBuilder.ForOpen(row.ActiveNormItem.Norm.Id));
			page.ViewModel.SelectItem(row.ActiveNormItem.Id);
		}

		public void OpenLastIssue(EmployeeCardItem row)
		{
			var referencedoc = employeeIssueRepository.GetReferencedDocuments(row.LastIssueOperation.Id);
			if (!referencedoc.Any() || referencedoc.First().DocumentType == null) {
				interactive.ShowMessage(ImportanceLevel.Error, "Не найдена ссылка на документ выдачи");
				return;
			}
			stockDocumentsModel.EditDocumentDialog(employeeViewModel, referencedoc.First());
		}

		public void RecalculateLastIssue(EmployeeCardItem row)
		{
			var operation = row.LastIssueOperation;
			//Если строку нормы по которой выдавали удалили, пытаемся пере-подвязать к имеющийся совпадающей по СИЗ 
			if (!row.EmployeeCard.WorkwearItems.Any(x => x.ActiveNormItem.IsSame(operation.NormItem))) {
				if (row.EmployeeCard.WorkwearItems.Any(x => x.ProtectionTools.Id == operation.ProtectionTools.Id)) {
					var norm = row.EmployeeCard.WorkwearItems
						.Where(x => x.ProtectionTools.Id == operation.ProtectionTools.Id)
						.Select(x => x.ActiveNormItem)
						.FirstOrDefault();
					if (norm != null)
						operation.NormItem = norm;
				}
			}
			operation.RecalculateDatesOfIssueOperation(row.Graph, BaseParameters, interactive);
			row.Graph.Refresh();
			row.UpdateNextIssue(UoW);
		}

		#endregion
		protected void RefreshWorkItems()
		{
			if(!NHibernateUtil.IsInitialized(Entity.WorkwearItems))
				return;

			foreach(var item in Entity.WorkwearItems) {
				UoW.Session.Refresh(item);
			}
			Entity.FillWearInStockInfo(UoW, BaseParameters, Entity.Subdivision?.Warehouse, DateTime.Now);
			Entity.FillWearReceivedInfo(employeeIssueRepository);
		}

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}
