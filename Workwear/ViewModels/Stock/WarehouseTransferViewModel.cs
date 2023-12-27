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
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Stock
{
	public class WarehouseTransferViewModel : EntityDialogViewModelBase<Transfer>
	{
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseToEntryViewModel;
		public readonly FeaturesService FeaturesService;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly IInteractiveQuestion interactive;
		
		public IList<Owner> Owners { get; }

		public WarehouseTransferViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigationManager, 
			ILifetimeScope autofacScope, 
			IValidator validator, 
			IUserService userService,
			BaseParameters baseParameters,
			StockBalanceModel stockBalanceModel,
			IInteractiveQuestion interactive,
			FeaturesService featuresService) : base(uowBuilder, unitOfWorkFactory, navigationManager, validator, unitOfWorkProvider) {
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser();

			var entryBuilder = new CommonEEVMBuilderFactory<Transfer>(this, Entity, UoW, navigationManager) {
				AutofacScope = autofacScope
			};
			WarehouseFromEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseFrom).MakeByType().Finish();
			WarehouseToEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseTo).MakeByType().Finish();
			
			Entity.PropertyChanged += Entity_PropertyChanged;
			Owners = UoW.GetAll<Owner>().ToList();

			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, 
					new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));

			this.FeaturesService = featuresService;
			
			//Заполняем складские остатки
			stockBalanceModel.Warehouse = Entity.WarehouseFrom;
			stockBalanceModel.OnDate = Entity.Date;
			if(Entity.Items.Any()) {
				stockBalanceModel.ExcludeOperations = Entity.Items.Select(x => x.WarehouseOperation).ToList();
				stockBalanceModel.AddNomenclatures(Entity.Items.Select(x => x.Nomenclature));
				foreach(var item in Entity.Items) {
					item.StockBalanceModel = this.stockBalanceModel;
				}
			}
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(Entity.WarehouseFrom) && Entity.WarehouseFrom != stockBalanceModel.Warehouse) {
				if(Entity.Items.Any()) {
					if(interactive.Question("При изменении склада отправителя строки документа будут очищены. Продолжить?")) {
						Entity.Items.Clear();
					}
					else {
						//Возвращаем назад старый склад
						Entity.WarehouseFrom = stockBalanceModel.Warehouse;
						return;
					}
				}
				
				stockBalanceModel.Warehouse = Entity.WarehouseFrom;
				OnPropertyChanged(nameof(CanAddItem));
			}

			if(e.PropertyName == nameof(Entity.Date))
				stockBalanceModel.OnDate = Entity.Date;
		}
		#region Sensetive
		public bool CanAddItem => Entity.WarehouseFrom != null;
		#endregion
		public void AddItems() {
			var selectPage = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.Filter.CanChooseAmount = true;
			selectPage.ViewModel.Filter.Warehouse = Entity.WarehouseFrom;
			selectPage.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectPage.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		private void ViewModel_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var addedAmount = ((StockBalanceJournalViewModel)sender).Filter.AddAmount;
			var items = new List<TransferItem>();
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var position = node.GetStockPosition(UoW);
				var item = Entity.AddItem(position,
					addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : node.Amount));
				if (item != null)
					items.Add(item);
			}
			stockBalanceModel.AddNomenclatures(items.Select(x => x.Nomenclature));
			foreach(var item in items) {
				item.StockBalanceModel = stockBalanceModel;
			}
		}
		public void RemoveItems(IEnumerable<TransferItem> items) {
			foreach(var item in items) {
				Entity.Items.Remove(item);
			}
		}
		public void OpenNomenclature(Nomenclature nomenclature) {
			NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(
				this, EntityUoWBuilder.ForOpen(nomenclature.Id));
		}
		public override bool Save() {
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
	}
}
