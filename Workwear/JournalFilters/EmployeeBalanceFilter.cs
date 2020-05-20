using System;
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
	public partial class EmployeeBalanceFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		public EmployeeBalanceFilter (IUnitOfWork uow, ITdiTab tab) : this()
		{
			UoW = uow;

			var AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();

			var builder = new LegacyEEVMBuilderFactory(tab, UoW, MainClass.MainWin.NavigationManager, AutofacScope);
			yentryEmployee.ViewModel = builder.ForEntity<EmployeeCard>()
					.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();
			yentryEmployee.ViewModel.ChangedByUser += Employee_Changed;
		}

		public EmployeeCard RestrictEmployee {
			get { return yentryEmployee.ViewModel.Entity as EmployeeCard; }
			set {
				yentryEmployee.ViewModel.Entity = value;
				yentryEmployee.Sensitive = false;
			}
		}

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}
			
		public EmployeeBalanceFilter ()
		{
			this.Build ();
		}

		void Employee_Changed(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}