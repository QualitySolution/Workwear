using System;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain.Stock;
using workwear.Repository;
using workwear.Domain;

namespace workwear
{
	public partial class WriteOffDocDlg : FakeTDIEntityGtkDialogBase<Writeoff>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public WriteOffDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Writeoff> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ConfigureDlg ();
		}

		public WriteOffDocDlg (EmployeeCard employee) : this () 
		{
			ItemsTable.CurWorker = employee;
		}

		public WriteOffDocDlg (Facility facility) : this () 
		{
			//Entity.Facility = facility;
			//FIXME
		}

		public WriteOffDocDlg (Writeoff item) : this (item.Id) {}

		public WriteOffDocDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Writeoff> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource ();

			ylabelCreatedBy.Binding.AddFuncBinding (Entity, e => e.CreatedbyUser.Name, w => w.LabelProp).InitializeFromSource ();

			ydateDoc.Binding.AddBinding (Entity, e => e.Date, w => w.Date).InitializeFromSource ();

			ItemsTable.WriteoffDoc = Entity;
		}			

		public override bool Save()
		{
			logger.Info ("Запись документа...");
			var valid = new QSValidation.QSValidator<Writeoff> (UoWGeneric.Root);
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

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if(Save ())
				Respond (Gtk.ResponseType.Ok);
		}
	}
}

