using System;
using System.Linq;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using workwear.Domain;
using workwear.Domain.Stock;
using workwear.Repository;

namespace workwear
{
	public partial class WriteOffDocDlg : OrmGtkDialogBase<Writeoff>
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
			ItemsTable.CurObject = facility;
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

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

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
				if(Entity.Items.Any (w => w.IssuedOn != null))
				{
					logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
					foreach(var employeeGroup in Entity.Items.Where (w => w.IssuedOn != null && w.IssuedOn.ExpenseDoc.EmployeeCard != null).GroupBy (w => w.IssuedOn.ExpenseDoc.EmployeeCard.Id))
					{
						var employee = employeeGroup.Select (eg => eg.IssuedOn.ExpenseDoc.EmployeeCard).First ();
						foreach(var itemsGroup in employeeGroup.GroupBy (i => i.Nomenclature.Type.Id))
						{
							var wearItem = employee.WorkwearItems.FirstOrDefault (i => i.Item.Id == itemsGroup.Key);
							if(wearItem == null)
							{
								logger.Debug ("Позиции <{0}> не требуется к выдаче, пропускаем...", itemsGroup.First ().Nomenclature.Type.Name);
								continue;
							}

							wearItem.UpdateNextIssue (UoW, itemsGroup.Select (i => i.IssuedOn).ToArray ());
						}
					}
					UoWGeneric.Commit ();
				}
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog ("Не удалось записать документ.", logger, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;
		}
	}
}

