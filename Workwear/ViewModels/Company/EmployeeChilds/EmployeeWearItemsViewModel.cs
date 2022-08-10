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
using workwear.Dialogs.Issuance;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Models.Stock;
using workwear.Repository.Operations;
using workwear.ViewModels.Operations;
using workwear.ViewModels.Regulations;
using workwear.Tools;
using workwear.ViewModels.Stock;
using workwear.Tools.Features;
using workwear.Domain.Operations.Graph;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeWearItemsViewModel : ViewModelBase, IDisposable
	{
		private readonly EmployeeViewModel employeeViewModel;
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly IInteractiveService interactive;
		private readonly ITdiCompatibilityNavigation navigation;
		private readonly OpenStockDocumentsModel stockDocumentsModel;

		public readonly BaseParameters BaseParameters;
		public EmployeeWearItemsViewModel(
			EmployeeViewModel employeeViewModel,
			EmployeeIssueRepository employeeIssueRepository,
			BaseParameters baseParameters,
			IInteractiveService interactive,
			ITdiCompatibilityNavigation navigation,
			OpenStockDocumentsModel stockDocumentsModel,
			FeaturesService featuresService)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.stockDocumentsModel = stockDocumentsModel ?? throw new ArgumentNullException(nameof(stockDocumentsModel));
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));

			employeeIssueRepository.RepoUow = UoW;
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<EmployeeCardItem>(HandleEntityChangeEvent);
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
				Entity.FillWearInStockInfo(UoW, BaseParameters, Entity.Subdivision?.Warehouse, DateTime.Now);
				Entity.FillWearRecivedInfo(employeeIssueRepository);
				OnPropertyChanged(nameof(ObservableWorkwearItems));
			}
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
			if(changeEvents.First().Session == UoW.Session)
				return; //Не чего не делаем если это наше собственное изменение.
			if(changeEvents.Where(x => x.EventType == TypeOfChangeEvent.Delete).Select(e => e.Entity).OfType<EmployeeCardItem>().Any(x => x.EmployeeCard.IsSame(Entity))) {
				//Если сделано удаление строк, просто закрываем диалог, так как заставить корректно сохранить сотрудника все равно не поучится.
				//Не работал следующий сценарий: Открываем диалог сотрудника, строка добавленная по норме есть в списке, открываем норму, удаляем одну из строк, сохраняем норму. После этого пытаемся сохранить сотрудника.
				var page = navigation.FindPage(employeeViewModel);
				navigation.ForceClosePage(page, CloseSource.Self);
				return;
			}

			if(changeEvents.Select(e => e.Entity).OfType<EmployeeCardItem>().Any(x => x.EmployeeCard.IsSame(Entity))) {
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
			navigation.OpenTdiTab<IncomeDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void OpenTimeLine(EmployeeCardItem item)
		{
			navigation.OpenTdiTab<EmployeeIssueGraphDlg, EmployeeCard, ProtectionTools>(employeeViewModel, Entity, item.ProtectionTools);
		}

		public void WriteOffWear()
		{
			navigation.OpenTdiTab<WriteOffDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void UpdateWorkwearItems()
		{
			Entity.UpdateWorkwearItems();
			Entity.FillWearInStockInfo(UoW, BaseParameters, Entity.Subdivision?.Warehouse, DateTime.Now);
			Entity.UpdateNextIssueAll();
		}

		public void SetIssueDateManual(EmployeeCardItem row)
		{
			var operations = employeeIssueRepository.GetOperationsForEmployee(UoW, Entity, row.ProtectionTools).OrderByDescending(x => x.OperationTime).ToList();
			IPage<ManualEmployeeIssueOperationViewModel> page;
			if(!operations.Any() || operations.First().ExpiryByNorm < DateTime.Today)
				page = navigation.OpenViewModel<ManualEmployeeIssueOperationViewModel, IEntityUoWBuilder, EmployeeCardItem>(employeeViewModel, EntityUoWBuilder.ForCreate(), row, OpenPageOptions.AsSlave);
			else if(operations.First().OverrideBefore)
				page = navigation.OpenViewModel<ManualEmployeeIssueOperationViewModel, IEntityUoWBuilder, EmployeeCardItem>(employeeViewModel, EntityUoWBuilder.ForOpen(operations.First().Id), row, OpenPageOptions.AsSlave);
			else if(interactive.Question($"Для «{row.ProtectionTools.Name}» уже выполнялись полноценные выдачи, внесение ручных изменений может привести к нежелательным результатам. Продолжить?"))
				page = navigation.OpenViewModel<ManualEmployeeIssueOperationViewModel, IEntityUoWBuilder, EmployeeCardItem>(employeeViewModel, EntityUoWBuilder.ForCreate(), row, OpenPageOptions.AsSlave);
			else
				return;
			page.PageClosed += SetIssueDateManual_PageClosed;
		}

		void SetIssueDateManual_PageClosed(object sender, PageClosedEventArgs e)
		{
			if(e.CloseSource == CloseSource.Save || e.CloseSource == CloseSource.Self) {
				var page = sender as IPage<ManualEmployeeIssueOperationViewModel>;
				var operation = UoW.GetById<EmployeeIssueOperation>(page.ViewModel.Entity.Id);
				if(operation != null)
					if(e.CloseSource == CloseSource.Self) //Self используется при удалении
					{
						UoW.Delete(operation);
						UoW.Commit();
					}
					else
						UoW.Session.Refresh(operation); //Почему то не срабатывает при втором вызове. Но не смог починить.
				Entity.FillWearRecivedInfo(employeeIssueRepository);
				Entity.UpdateNextIssue(page.ViewModel.Entity.ProtectionTools);
			}
		}
		#endregion
		#region Контекстное меню
		public void OpenProtectionTools(EmployeeCardItem row)
		{
			navigation.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(employeeViewModel, EntityUoWBuilder.ForOpen(row.ProtectionTools.Id));
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
			var graph = IssueGraph.MakeIssueGraph(UoW, row.EmployeeCard, operation.ProtectionTools);
			operation.RecalculateDatesOfIssueOperation(graph, BaseParameters, interactive);
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
			Entity.FillWearRecivedInfo(employeeIssueRepository);
		}

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}