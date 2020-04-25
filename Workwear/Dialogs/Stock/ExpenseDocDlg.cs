﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository;
using workwear.ViewModels.Statements;
using workwear.ViewModels.Stock;

namespace workwear
{
	public partial class ExpenseDocDlg : EntityDialogBase<Expense>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		ILifetimeScope AutofacScope;

		public ExpenseDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Expense> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ConfigureDlg ();
		}

		public ExpenseDocDlg (Subdivision subdivision) : this () {
			Entity.Operation = ExpenseOperations.Object;
			Entity.Subdivision = subdivision;
			Entity.Warehouse = subdivision.Warehouse;
		}

		public ExpenseDocDlg (EmployeeCard employee, bool fillUnderreceived = false) : this () {
			Entity.Operation = ExpenseOperations.Employee;
			Entity.Employee = UoW.GetById<EmployeeCard> (employee.Id);
			Entity.Warehouse = Entity.Employee.Subdivision?.Warehouse;
			if(fillUnderreceived)
				FillUnderreceived();
		}

		private void FillUnderreceived()
		{
			Entity.Employee.FillWearInStockInfo(UoW, Entity.Warehouse, Entity.Date, onlyUnderreceived: true);

			foreach(var item in Entity.Employee.UnderreceivedItems)
			{
				if(item.InStockState != StockStateInfo.Enough && item.InStockState != StockStateInfo.NotEnough)
				{
					logger.Warn($"На складе отсутствуют позиции для {item.Item.Name}. Пропускаем.");
					continue;
				}
				var stockBalance = item.BestChoiceInStock.First();
				var amount = item.InStockState == StockStateInfo.Enough ? item.NeededAmount : stockBalance.Amount;
				Entity.AddItem (stockBalance.StockPosition, amount);
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
			yentryEmployee.Binding.AddBinding (Entity, e => e.Employee, w => w.Subject).InitializeFromSource ();

			yentryObject.SubjectType = typeof(Subdivision);
			yentryObject.Binding.AddBinding (Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource ();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ItemsTable.ExpenceDoc = Entity;

			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();

			var builder = new LegacyEEVMBuilderFactory<Expense>(this, Entity, UoW, MainClass.MainWin.NavigationManager, AutofacScope);


			entityWarehouseExpense.ViewModel = builder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();

			Entity.PropertyChanged += Entity_PropertyChanged;

			IssuanceSheetSensetive();
		}			

		public override bool Save()
		{
			logger.Info ("Запись документа...");
			var valid = new QSValidator<Expense> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			var ask = new GtkQuestionDialogsInteractive();
			Entity.UpdateOperations(UoW, ask);
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);
			UoWGeneric.Save ();
			if(Entity.Operation == ExpenseOperations.Employee)
			{
				logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
				Entity.UpdateEmployeeNextIssue();
				UoWGeneric.Commit ();
			}
			
			logger.Info ("Ok");
			return true;
		}
			
		protected void OnYcomboOperationChanged (object sender, EventArgs e)
		{
			labelWorker.Visible = yentryEmployee.Visible =
				labelIssuanceSheet.Visible = hboxIssuanceSheet.Visible =
				Entity.Operation == ExpenseOperations.Employee;
			labelObject.Visible = yentryObject.Visible = 
				Entity.Operation == ExpenseOperations.Object;
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Employee))
				IssuanceSheetSensetive();
		}

		public override void Destroy()
		{
			base.Destroy();
			AutofacScope.Dispose();
		}

		#region Ведомости

		private void IssuanceSheetSensetive()
		{
			buttonIssuanceSheetCreate.Sensitive = Entity.Employee != null;
			buttonIssuanceSheetCreate.Visible = Entity.IssuanceSheet == null;
			buttonIssuanceSheetOpen.Visible = buttonIssuanceSheetPrint.Visible = Entity.IssuanceSheet != null;
		}

		protected void OnButtonIssuanceSheetCreateClicked(object sender, EventArgs e)
		{
			Entity.CreateIssuanceSheet();
			IssuanceSheetSensetive();
		}

		protected void OnButtonIssuanceSheetOpenClicked(object sender, EventArgs e)
		{
			if(UoW.HasChanges) {
				if(MessageDialogHelper.RunQuestionDialog("Сохранить документ выдачи перед открытием ведомости?"))
					Save();
				else
					return;
			}

			MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		protected void OnButtonIssuanceSheetPrintClicked(object sender, EventArgs e)
		{
			if(UoW.HasChanges) {
				if(CommonDialogs.SaveBeforePrint(Entity.GetType(), "ведомости"))
					Save();
				else
					return;
			}

			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.Id),
				Identifier = "Statements.IssuanceSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id }
				}
			};
			MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion
	}
}

