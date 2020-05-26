using System;
using Autofac;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;
using workwear.ViewModels.Company;

namespace workwear
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ObjectBalanceFilter : Gtk.Bin, IRepresentationFilter
	{
		ILifetimeScope AutofacScope;

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();

				Func<ITdiTab> getTab = () => DialogHelper.FindParentTab(this);

				var builder = new LegacyEEVMBuilderFactory(getTab, uow, MainClass.MainWin.NavigationManager, AutofacScope);

				entitySubdivision.ViewModel = builder.ForEntity<Subdivision>()
					.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
					.UseViewModelDialog<SubdivisionViewModel>()
					.Finish();

				entitySubdivision.ViewModel.ChangedByUser += OnYentryObjectChanged;
			}
		}
			
		public Subdivision RestrictObject {
			get { return entitySubdivision.ViewModel.Entity as Subdivision; }
			set {
				entitySubdivision.ViewModel.Entity = value;
				entitySubdivision.ViewModel.IsEditable = false;
			}
		}

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public ObjectBalanceFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}
			
		public ObjectBalanceFilter ()
		{
			this.Build ();
		}

		protected void OnYentryObjectChanged(object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		public override void Destroy()
		{
			AutofacScope.Dispose();
			base.Destroy();
		}
	}
}

