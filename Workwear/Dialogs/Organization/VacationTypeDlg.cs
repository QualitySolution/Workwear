using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using workwear.Domain.Company;

namespace workwear.Dialogs.Organization
{
	public partial class VacationTypeDlg : EntityDialogBase<VacationType>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public VacationTypeDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<VacationType>();
			ConfigureDlg();
		}

		public VacationTypeDlg(VacationType type) : this(type.Id) { }

		public VacationTypeDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<VacationType>(id);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ycheckExcludeTime.Binding.AddBinding(Entity, e => e.ExcludeFromWearing, w => w.Active).InitializeFromSource();

			ytextviewComments.Binding.AddBinding(Entity, e => e.Comments, w => w.Buffer.Text).InitializeFromSource();
		}

		public override bool Save()
		{
			var valid = new QS.Validation.GtkUI.QSValidator<VacationType>(Entity);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}
	}
}
