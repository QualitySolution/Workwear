using System;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Tools;

namespace workwear.Dialogs.Organization
{
	public partial class EmployeeVacationDlg : EntityDialogBase<EmployeeVacation>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public EmployeeVacationDlg(EmployeeCard employee)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeVacation>();
			var loadedEmployee = UoW.GetById<EmployeeCard>(employee.Id);
			loadedEmployee.AddVacation(Entity);
			ConfigureDlg();
		}

		public EmployeeVacationDlg(EmployeeVacation vacation) : this(vacation.Id) { }

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
			var valid = new QS.Validation.QSValidator<EmployeeVacation>(Entity);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			var baseParameters = new BaseParameters(UoW.Session.Connection);
			Entity.UpdateRelatedOperations(UoW, new Repository.Operations.EmployeeIssueRepository(), baseParameters, new GtkQuestionDialogsInteractive());
			UoW.Save(Entity.Employee);

			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}
	}
}
