using System;
using Autofac;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.ViewModels.Control.EEVM;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Stock;
using workwear.JournalViewModels.Stock;
using workwear.ViewModels.Stock;

namespace workwear.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WarehouseFilter : Gtk.Bin, IRepresentationFilter
	{
		ILifetimeScope AutofacScope;

		IUnitOfWork uow;


		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		public Warehouse RestrictWarehouse {

			get { return entityWarehouse.ViewModel?.Entity as Warehouse; }
			set {
				entityWarehouse.ViewModel.Entity = value;
				entityWarehouse.Sensitive = false;
			}


		}
		public event EventHandler Refiltered;

		void OnRefiltered()
		{
			if(Refiltered != null)
				Refiltered(this, new EventArgs());
		}


		public WarehouseFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public WarehouseFilter()
		{
			this.Build();
		}

		protected void OnYentryObjectChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected override void OnShown()
		{
			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			var builder = new LegacyEEVMBuilderFactory(DialogHelper.FindParentTab(this), uow, MainClass.MainWin.NavigationManager, AutofacScope);

			entityWarehouse.ViewModel = builder.ForEntity<Warehouse>()
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();

			entityWarehouse.ViewModel.ChangedByUser += Warehouse_ChangedByUser;
			

			base.OnShown();
		}

		void Warehouse_ChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnChBtWarehouseClicked(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		public bool RestrictOnlyNull {

			get { return chBtWarehouse.Active; }
			set {
				chBtWarehouse.Active = value;
				chBtWarehouse.Sensitive = false;
			}
		}
	}
}
