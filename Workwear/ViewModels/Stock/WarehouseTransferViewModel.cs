using System;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QSOrmProject;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.JournalViewModels.Stock;

namespace workwear.ViewModels.Stock
{
	public class WarehouseTransferViewModel : LegacyEntityDialogViewModelBase<Transfer>
	{
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseToEntryViewModel;
		public ILifetimeScope AutofacScope;
		public ITdiCompatibilityNavigation navigationManager;
		public ITdiCompatibilityNavigation tdiNavigationManager;
		private readonly CommonMessages commonMessages;

		public WarehouseTransferViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ITdiTab myTab, ITdiCompatibilityNavigation navigationManager, ILifetimeScope autofacScope, CommonMessages commonMessages, IUserService userService) : base(uowBuilder, unitOfWorkFactory, myTab, navigationManager)
		{
			this.tdiNavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.commonMessages = commonMessages;

			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);

			var entryBuilder = new LegacyEEVMBuilderFactory<Transfer>(this, TdiTab, Entity, UoW, navigationManager) {
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


		}


		#region Sensetive

		public bool CanEditItems => Entity.CreatedbyUser == null;

		#endregion

		public void AddItems()
		{
			var selectPage = tdiNavigationManager.OpenTdiTab<OrmReference, Type>(TdiTab, typeof(Nomenclature), OpenPageOptions.AsSlave);

			var selectDialog = selectPage.TdiTab as OrmReference;
			selectDialog.Mode = OrmReferenceMode.MultiSelect;
			selectDialog.ObjectSelected += NomenclatureJournal_ObjectSelected;
		}

		void NomenclatureJournal_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var nomenclature in e.GetEntities<Nomenclature>()) {
				Entity.AddItem(nomenclature);
			}
		}

		public void RemoveItems(TransferItem[] items)
		{
			foreach(var item in items) {
				Entity.ObservableItems.Remove(item);
			}
		}

		public void SetNomenclature(TransferItem[] items)
		{
			var selectPage = tdiNavigationManager.OpenTdiTab<OrmReference, Type>(TdiTab, typeof(Nomenclature), OpenPageOptions.AsSlave);

			var selectDialog = selectPage.TdiTab as OrmReference;
			selectDialog.Tag = items;
			selectDialog.Mode = OrmReferenceMode.Select;
			selectDialog.ObjectSelected += SetNomenclature_ObjectSelected;
		}

		void SetNomenclature_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var items = (sender as OrmReference).Tag as TransferItem[];
			foreach(var item in items) {
				item.Nomenclature = e.Subject as Nomenclature;
			}
		}

		public override bool Save()
		{
			Entity.UpdateOperations(UoW, null); 
			return base.Save();
		}

		public override void Dispose()
		{
			base.Dispose();
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}
