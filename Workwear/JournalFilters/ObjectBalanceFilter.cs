using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;
using workwear.Repository.Company;

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
				yentryObject.ItemsQuery = SubdivisionRepository.ActiveObjectsQuery ();
			}
		}
			
		public Subdivision RestrictObject {
			get { return yentryObject.Subject as Subdivision; }
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

