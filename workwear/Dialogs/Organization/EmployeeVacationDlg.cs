using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using workwear.Domain.Organization;

namespace workwear.Dialogs.Organization
{
	public partial class EmployeeVacationDlg : EntityDialogBase<EmployeeVacation>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public EmployeeVacationDlg(EmployeeCard employee)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeVacation>();
			Entity.Employee = UoW.GetById<EmployeeCard>(employee.Id);
			ConfigureDlg();
		}

		public EmployeeVacationDlg(EmployeeVacation card) : this(card.Id) { }

		public EmployeeVacationDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<EmployeeVacation>(id);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryVacationType.SubjectType = typeof(VacationType);
			yentryVacationType.Binding.AddBinding(Entity, e => e.VacationType, w => w.Subject).InitializeFromSource();

			ydateperiodVacation.Binding.AddSource(Entity)
				.AddBinding(e => e.BeginDate, w => w.StartDate)
				.AddBinding(e => e.EndDate, w => w.EndDate)
				.InitializeFromSource();

			ytextviewComments.Binding.AddBinding(Entity, e => e.Comments, w => w.Buffer.Text).InitializeFromSource();
		}

		public override bool Save()
		{
			var valid = new QSValidation.QSValidator<EmployeeVacation>(Entity);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}
	}
}
