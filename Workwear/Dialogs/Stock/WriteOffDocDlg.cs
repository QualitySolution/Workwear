using System;
using System.Linq;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Repository;
using workwear.Repository.Stock;

namespace workwear
{
	public partial class WriteOffDocDlg : EntityDialogBase<Writeoff>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public WriteOffDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Writeoff> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ItemsTable.CurWarehouse = new StockRepository().GetDefaultWarehouse(UoW,new Tools.Features.FeaturesService());
			ConfigureDlg ();
		}

		public WriteOffDocDlg (EmployeeCard employee) : this () 
		{
			ItemsTable.CurWorker = employee;
		}

		public WriteOffDocDlg (Subdivision facility) : this () 
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
			var valid = new QS.Validation.QSValidator<Writeoff> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			
			Entity.UpdateOperations(UoW);
			UoWGeneric.Save ();
			if(Entity.Items.Any (w => w.WriteoffFrom == WriteoffFrom.Employye))
			{
				logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
				foreach(var employeeGroup in Entity.Items.Where (w => w.WriteoffFrom == WriteoffFrom.Employye).GroupBy (w => w.EmployeeWriteoffOperation.Employee))
				{
					var employee = employeeGroup.Key;
					foreach(var itemsGroup in employeeGroup.GroupBy (i => i.Nomenclature.Type.Id))
					{
						var wearItem = employee.WorkwearItems.FirstOrDefault (i => i.ProtectionTools.Id == itemsGroup.Key);
						if(wearItem == null)
						{
							logger.Debug ("Позиции <{0}> не требуется к выдаче, пропускаем...", itemsGroup.First ().Nomenclature.Type.Name);
							continue;
						}

						wearItem.UpdateNextIssue (UoW);
					}
				}
				UoWGeneric.Commit ();
			}
			
			logger.Info ("Ok");
			return true;
		}
	}
}