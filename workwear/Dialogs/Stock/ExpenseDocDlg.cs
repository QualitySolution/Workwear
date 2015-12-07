using System;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using workwear.Domain;
using workwear.Domain.Stock;
using workwear.Repository;

namespace workwear
{
	public partial class ExpenseDocDlg : FakeTDIEntityGtkDialogBase<Expense>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ExpenseDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Expense> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ConfigureDlg ();
		}

		public ExpenseDocDlg (Facility facility) : this () {
			Entity.Operation = ExpenseOperations.Object;
			Entity.Facility = facility;
		}

		public ExpenseDocDlg (EmployeeCard employee) : this () {
			Entity.Operation = ExpenseOperations.Employee;
			Entity.EmployeeCard = employee;
		}

		public ExpenseDocDlg (Expense item) : this (item.Id) {}

		public ExpenseDocDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Expense> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			ylabelCreatedBy.Binding.AddFuncBinding (Entity, e => e.CreatedbyUser.Name, w => w.LabelProp).InitializeFromSource ();

			ydateDoc.Binding.AddBinding (Entity, e => e.Date, w => w.Date).InitializeFromSource ();

			ycomboOperation.ItemsEnum = typeof(ExpenseOperations);
			ycomboOperation.Binding.AddBinding (Entity, e => e.Operation, w => w.SelectedItemOrNull).InitializeFromSource ();

			yentryEmployee.SubjectType = typeof(EmployeeCard);
			yentryEmployee.Binding.AddBinding (Entity, e => e.EmployeeCard, w => w.Subject).InitializeFromSource ();

			yentryObject.SubjectType = typeof(Facility);
			yentryObject.Binding.AddBinding (Entity, e => e.Facility, w => w.Subject).InitializeFromSource ();
		}			

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if(Save ())
				Respond (Gtk.ResponseType.Ok);
		}

		public override bool Save()
		{
			logger.Info ("Запись документа...");
			var valid = new QSValidator<Expense> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			try {
				UoWGeneric.Save ();
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog (this, "Не удалось записать документ.", logger, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;
		}
			
		protected void OnYcomboOperationChanged (object sender, EventArgs e)
		{
			labelWorker.Visible = yentryEmployee.Visible =
				Entity.Operation == ExpenseOperations.Employee;
			labelObject.Visible = yentryObject.Visible = 
				Entity.Operation == ExpenseOperations.Object;
		}
	}
}

