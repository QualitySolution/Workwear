using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;

namespace Workwear.ViewModels.Stock
{
	public class WarehouseTransferViewModel : EntityDialogViewModelBase<Transfer>
	{
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseToEntryViewModel;
		private ILifetimeScope AutofacScope;
		private readonly IInteractiveQuestion interactive;

		private Warehouse lastWarehouse;

		public WarehouseTransferViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigationManager, 
			ILifetimeScope autofacScope, 
			IValidator validator, 
			IUserService userService,
			BaseParameters baseParameters,
			IInteractiveQuestion interactive) : base(uowBuilder, unitOfWorkFactory, navigationManager, validator) {
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);

			var entryBuilder = new CommonEEVMBuilderFactory<Transfer>(this, Entity, UoW, navigationManager) {
				AutofacScope = AutofacScope
			};
			WarehouseFromEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseFrom)
										 .UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
										 .UseViewModelDialog<WarehouseViewModel>()
										 .Finish();
			WarehouseToEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseTo)
										 .UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
										 .UseViewModelDialog<WarehouseViewModel>()
										 .Finish();
			LoadActualAmountFromStock();
			Entity.PropertyChanged += Entity_PropertyChanged;
			lastWarehouse = Entity.WarehouseFrom;

			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, 
					new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName != nameof(Entity.WarehouseFrom) || Entity.WarehouseFrom == lastWarehouse) return;
			if(Entity.Items.Any()) {
				if(interactive.Question("При изменении склада отправителя строки документа будут очищены. Продолжить?")) {
					Entity.ObservableItems.Clear();
				}
				else { //Возвращаем назад старый склад
					Entity.WarehouseFrom = lastWarehouse;
					return;
				}
			}
			lastWarehouse = Entity.WarehouseFrom;
			OnPropertyChanged(nameof(CanAddItem));
		}
		#region Sensetive
		public bool CanAddItem => Entity.WarehouseFrom != null;
		#endregion
		public void AddItems() {
			var selectPage = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.Filter.Warehouse = Entity.WarehouseFrom;
			selectPage.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectPage.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		private void ViewModel_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			foreach(var stockBalance in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				Entity.AddItem(stockBalance.GetStockPosition(UoW), stockBalance.Amount);
			}
		}
		public void RemoveItems(IEnumerable<TransferItem> items) {
			foreach(var item in items) {
				Entity.ObservableItems.Remove(item);
			}
		}
		public void OpenNomenclature(Nomenclature nomenclature) {
			NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(
				this, EntityUoWBuilder.ForOpen(nomenclature.Id));
		}
		public override bool Save() {
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			Entity.UpdateOperations(UoW, null); 
			return base.Save();
		}
		public override void Dispose() {
			base.Dispose();
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
		public bool ValidateNomenclature(TransferItem transferItem) {
			return transferItem.Amount <= transferItem.AmountInStock;
		}
		private void LoadActualAmountFromStock() {
			Entity.SetAmountInStock();
		}
	}
}
