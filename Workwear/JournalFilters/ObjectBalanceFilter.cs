using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;

namespace workwear
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ObjectBalanceFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				yentryObject.ItemsQuery = Repository.FacilityRepository.ActiveObjectsQuery ();
			}
		}
			
		public Facility RestrictObject {
			get { return yentryObject.Subject as Facility; }
			set {
				yentryObject.Subject = value;
				yentryObject.Sensitive = false;
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
	}
}

