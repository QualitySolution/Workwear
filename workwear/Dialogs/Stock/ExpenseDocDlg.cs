using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using workwear.Domain.Organization;
using workwear.Domain.Stock;
using workwear.Repository;

namespace workwear
{
	public partial class ExpenseDocDlg : EntityDialogBase<Expense>
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
			Entity.EmployeeCard = UoW.GetById<EmployeeCard> (employee.Id);
		}

		public ExpenseDocDlg (EmployeeCard employee, Dictionary<Nomenclature, int> addItems) : this (employee) {
			
			var stock = StockRepository.BalanceInStockDetail (UoW, addItems.Keys.ToList ());

			foreach(var pair in addItems)
			{
				var inStock = stock.FirstOrDefault (s => s.Nomenclature == pair.Key);
				if(inStock == null)
				{
					logger.Warn("Количество по складу для номенклатуры {0}, не получено. Пропускаем.");
					continue;
				}
				var incomeItem = UoW.GetById<IncomeItem> (inStock.IncomeItemId);
				Entity.AddItem (incomeItem, pair.Value);
			}
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

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ItemsTable.ExpenceDoc = Entity;
		}			

		public override bool Save()
		{
			logger.Info ("Запись документа...");
			var valid = new QSValidator<Expense> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			try 
			{
				var ask = new GtkQuestionDialogsInteractive();
				Entity.UpdateOperations(UoW, ask);
				UoWGeneric.Save ();
				if(Entity.Operation == ExpenseOperations.Employee)
				{
					logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
					foreach(var itemsGroup in Entity.Items.GroupBy (i => i.Nomenclature.Type.Id))
					{
						var wearItem = Entity.EmployeeCard.WorkwearItems.FirstOrDefault (i => i.Item.Id == itemsGroup.Key);
						if(wearItem == null)
						{
							logger.Debug ("Позиции <{0}> не требуется к выдаче, пропускаем...", itemsGroup.First ().Nomenclature.Type.Name);
							continue;
						}

						wearItem.UpdateNextIssue (UoW);
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
			
		protected void OnYcomboOperationChanged (object sender, EventArgs e)
		{
			labelWorker.Visible = yentryEmployee.Visible =
				Entity.Operation == ExpenseOperations.Employee;
			labelObject.Visible = yentryObject.Visible = 
				Entity.Operation == ExpenseOperations.Object;
		}
	}
}

