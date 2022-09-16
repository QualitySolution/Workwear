using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Stock
{
	public class CompletionViewModel: EntityDialogViewModelBase<Completion>
	{
		private readonly IInteractiveQuestion interactive;
		private readonly FeaturesService featuresService;
		private Warehouse lastWarehouse;
		public SizeService SizeService { get; }
		public CompletionViewModel(IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			StockRepository stockRepository,
			FeaturesService featuresService,
			ILifetimeScope autofacScope,
			BaseParameters baseParameters,
			IInteractiveQuestion interactive,
			SizeService sizeService,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			var entryBuilder = new CommonEEVMBuilderFactory<Completion>(this, Entity, UoW, navigation, autofacScope);
			this.interactive = interactive;
			this.featuresService = featuresService;
			SizeService = sizeService;
			
			if(UoW.IsNew) 
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			
			if(Entity.SourceWarehouse == null)
				Entity.SourceWarehouse = stockRepository.GetDefaultWarehouse
					(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			
			WarehouseExpenseEntryViewModel = entryBuilder.ForProperty(x => x.SourceWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			if(Entity.ResultWarehouse == null)
				Entity.ResultWarehouse = stockRepository.GetDefaultWarehouse
					(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			WarehouseReceiptEntryViewModel = entryBuilder.ForProperty(x => x.ResultWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, 
				new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters }, {nameof(IUnitOfWork), UoW} })));
			Entity.PropertyChanged += Entity_PropertyChanged;
			lastWarehouse = Entity.SourceWarehouse;
		}

		#region View
		public bool CanDelItemSource => Entity.ObservableSourceItems.Count > 0;
		public bool CanDelItemResult => Entity.ObservableResultItems.Count > 0;
		public bool ShowWarehouses => featuresService.Available(WorkwearFeature.Warehouses);
		#endregion
		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseExpenseEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseReceiptEntryViewModel;

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(Entity.SourceWarehouse) || Entity.SourceWarehouse == lastWarehouse || Entity.SourceItems is null) return;
			if(Entity.SourceItems.Any()) {
				if(interactive.Question("При изменении склада комплектующих строки документа будут очищены. Продолжить?")) {
					Entity.ObservableSourceItems.Clear();
				}
				else { //Возвращаем назад старый склад
					Entity.SourceWarehouse = lastWarehouse;
					return;
				}
			}
			OnPropertyChanged(nameof(CanDelItemSource));
			lastWarehouse = Entity.SourceWarehouse;
		}
		#endregion
		#region Items
		public void AddSourceItems(){
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<StockBalanceJournalViewModel>(this, QS.Navigation.OpenPageOptions.AsSlave);
			if (Entity.SourceWarehouse != null) {
				selectJournal.ViewModel.Filter.Warehouse = Entity.SourceWarehouse;
				selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			}

			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}

		private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var selectVM = sender as StockBalanceJournalViewModel;
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				Entity.AddSourceItem(node.GetStockPosition(UoW), selectVM.Filter.Warehouse, node.Amount);
			}
		}
		public void AddResultItems() {
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<NomenclatureJournalViewModel>(this, QS.Navigation.OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature_OnSelectResult;
		}

		private void AddNomenclature_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddResultItem(n));
		}
		public void DelSourceItems(CompletionSourceItem item) {
			Entity.ObservableSourceItems.Remove(item);
			OnPropertyChanged(nameof(CanDelItemSource));
		}
		public void DelResultItems(CompletionResultItem item) {
			Entity.ObservableResultItems.Remove(item);
			OnPropertyChanged(nameof(CanDelItemResult));
		}
		#endregion
		#region Save
		public override bool Save() {
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			Entity.UpdateItems();
			return base.Save();
		}
		#endregion
	}
}
