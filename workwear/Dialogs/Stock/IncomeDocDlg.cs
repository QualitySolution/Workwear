using System;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain.Stock;
using workwear.Repository;
using workwear.Domain;
using QSValidation;

namespace workwear
{
	public partial class IncomeDocDlg : FakeTDIEntityGtkDialogBase<Income>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		string DocName;

		public IncomeDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ConfigureDlg ();
		}

		public IncomeDocDlg (EmployeeCard employee) : this () 
		{
			Entity.Operation = IncomeOperations.Return;
			Entity.EmployeeCard = employee;
		}

		public IncomeDocDlg (Facility facility) : this () 
		{
			Entity.Operation = IncomeOperations.Object;
			Entity.Facility = facility;
		}

		public IncomeDocDlg (Income item) : this (item.Id) {}

		public IncomeDocDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			ylabelCreatedBy.Binding.AddFuncBinding (Entity, e => e.CreatedbyUser.Name, w => w.LabelProp).InitializeFromSource ();

			ydateDoc.Binding.AddBinding (Entity, e => e.Date, w => w.Date).InitializeFromSource ();

			yentryNumber.Binding.AddBinding (Entity, e => e.Number, w => w.Text).InitializeFromSource ();

			ycomboOperation.ItemsEnum = typeof(IncomeOperations);
			ycomboOperation.Binding.AddBinding (Entity, e => e.Operation, w => w.SelectedItemOrNull).InitializeFromSource ();

			yentryEmployee.SubjectType = typeof(EmployeeCard);
			yentryEmployee.Binding.AddBinding (Entity, e => e.EmployeeCard, w => w.Subject).InitializeFromSource ();

			yentryObject.SubjectType = typeof(Facility);
			yentryObject.Binding.AddBinding (Entity, e => e.Facility, w => w.Subject).InitializeFromSource ();

			ItemsTable.IncomeDoc = Entity;
		}			

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if(Save ())
				Respond (Gtk.ResponseType.Ok);
		}

		public override bool Save()
		{
			logger.Info ("Запись документа...");
			var valid = new QSValidator<Income> (UoWGeneric.Root);
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
			switch (Entity.Operation)
			{
			case IncomeOperations.Enter:
				DocName = "Приходная накладная № ";
				this.Title = "Новая приходная накладная";
				break;
			case IncomeOperations.Return:
				DocName = "Возврат от работника № ";
				this.Title = "Новый возврат от работника";
				break;
			case IncomeOperations.Object:
				DocName = "Возврат c объекта № ";
				this.Title = "Новый возврат c объекта";
				break;
			}

			labelTTN.Visible = yentryNumber.Visible = Entity.Operation == IncomeOperations.Enter;
			labelWorker.Visible = yentryEmployee.Visible = Entity.Operation == IncomeOperations.Return;
			labelObject.Visible = yentryObject.Visible = Entity.Operation == IncomeOperations.Object;
		}
	}
}

