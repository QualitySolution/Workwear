using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;

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
				yentryEmployee.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeesQuery ();
			}
		}

		public EmployeeBalanceFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public EmployeeCard RestrictEmployee {
			get { return yentryEmployee.Subject as EmployeeCard; }
			set {
				yentryEmployee.Subject = value;
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

		protected void OnYentryEmployeeChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}
	}
}

