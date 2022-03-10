using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository.Stock;
using workwear.Tools.Features;

namespace workwear.ViewModels.Stock
{
	public class CompletionViewModel: EntityDialogViewModelBase<Completion>
	{
		public CompletionViewModel(IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			StockRepository stockRepository,
			FeaturesService featuresService,
			ILifetimeScope autofacScope,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			var entryBuilder = new CommonEEVMBuilderFactory<Completion>(this, Entity, UoW, navigation, autofacScope);
			
			if(UoW.IsNew) 
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			
			if(Entity.SourceWarehouse == null)
				Entity.SourceWarehouse = stockRepository.GetDefaultWarehouse
					(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			
			WarehouseExpenseEntryViewModel = entryBuilder.ForProperty(x => x.SourceWarehouse).MakeByType().Finish();
			
			if(Entity.ResultWarehouse == null)
				Entity.ResultWarehouse = stockRepository.GetDefaultWarehouse
					(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			WarehouseReceiptEntryViewModel = entryBuilder.ForProperty(x => x.ResultWarehouse).MakeByType().Finish();
		}
		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseExpenseEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseReceiptEntryViewModel;
		#endregion
		public void AddSourceItems(){
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<StockBalanceJournalViewModel>(this, QS.Navigation.OpenPageOptions.AsSlave);
			if(Entity.SourceWarehouse != null) 
				selectJournal.ViewModel.Filter.Warehouse = Entity.SourceWarehouse;
			
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}
		void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
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
		void AddNomenclature_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddResultItem(n));
		}
		public override bool Save() {
			Entity.UpdateItems();
			return base.Save();
		}
	}
}