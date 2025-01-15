using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Stock {
	public class ExpenseDutyNormViewModel : EntityDialogViewModelBase<ExpenseDutyNorm>{
		
		private ILifetimeScope autofacScope;
		private readonly IInteractiveService interactive;
		private readonly StockRepository stockRepository;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public SizeService SizeService { get; }
//Возможно стоит хранить в объекте ,собирать в конструкторе
		public IEnumerable<ProtectionTools> ProtectionToolsListFromNorm => Entity.DutyNorm.ProtectionToolsList;

		public ExpenseDutyNormViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope autofacScope, 
			INavigationManager navigation,
			IInteractiveService interactive, 
			IUserService userService,
			SizeService sizeService, 
			FeaturesService featutesService,
			StockRepository stockRepository,
			DutyNorm dutyNorm,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) 
		{			
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));

			Entity.DutyNorm = dutyNorm;
			var entryBuilder = new CommonEEVMBuilderFactory<ExpenseDutyNorm>(this, Entity, UoW, navigation, autofacScope);
			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featutesService, autofacScope.Resolve<IUserService>().CurrentUserId);
			if(Entity.Id == 0) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
			}
			
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			ResponsibleEmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.ResponsibleEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
			DutyNormEntryViewModel = entryBuilder.ForProperty(x => x.DutyNorm)
				.UseViewModelJournalAndAutocompleter<DutyNormsJournalViewModel>()
				.UseViewModelDialog<DutyNormViewModel>()
				.Finish();
		}

		#region EntityViewModels
		public readonly EntityEntryViewModel<DutyNorm> DutyNormEntryViewModel;
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public readonly EntityEntryViewModel<EmployeeCard> ResponsibleEmployeeCardEntryViewModel;
		#endregion
		
		public void AddItems() {
			if(Entity.Warehouse is null || Entity.DutyNorm is null) {
				interactive.ShowMessage(ImportanceLevel.Warning,
					(Entity.Warehouse is null ? "Склад должен быть указан.\n" : "" ) + 
					(Entity.DutyNorm is null ? "Дежурная норма должна быть указана.\n": ""));
				return;
			}
				
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>
				(this, QS.Navigation.OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.Warehouse = Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadItems;
		}

		private void LoadItems(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var stockPosition = node.GetStockPosition(UoW);
				var item = Entity.AddItem(stockPosition);
			}
		}

		public void DeleteItem(ExpenseDutyNormItem item) {
			Entity.RemoveItem(item);
		}

		#region Валидация и проверка
		public override bool Save()
		{
			logger.Info("Проверка документа...");
			if(!Validate()) {
				logger.Warn("Документ не корректен, сохранение отменено.");
				return false;
			}
			
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;

			foreach(var item in Entity.Items) 
				if(item.ProtectionTools != null) 
					item.DutyNormItem = Entity.DutyNorm.Items.First(x => x.ProtectionTools == item.ProtectionTools);

			Entity.UpdateOperations(UoW, interactive);
			UoW.Session.SaveOrUpdate(Entity);
			UoW.Commit();
			
			return true;
		}
		#endregion
	}
}
