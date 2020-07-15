using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.ViewModels.Company;
using workwear.ViewModels.Statements;

namespace workwear.ViewModels.Stock
{
	public class ExpenseEmployeeViewModel : EntityDialogViewModelBase<Expense>
	{
		ILifetimeScope autofacScope;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsEmployeeViewModel DocItemsEmployeeViewModel;

		public ExpenseEmployeeViewModel(IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IValidator validator,
			IUserService userService,
			IInteractiveQuestion interactive,
			EmployeeCard employee = null, 
			bool fillUnderreceived = false) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var entryBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, navigation, autofacScope);

			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);

			if (employee!= null) {
				Entity.Operation = ExpenseOperations.Employee;
				Entity.Employee = UoW.GetById<EmployeeCard>(employee.Id);
				Entity.Warehouse = Entity.Employee.Subdivision?.Warehouse;
				if(fillUnderreceived)
					FillUnderreceived();
			}
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();
			EmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.Employee)
									.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
									.UseViewModelDialog<EmployeeViewModel>()
									.Finish();

			var parameter = new TypedParameter(typeof(ExpenseEmployeeViewModel), this);
			DocItemsEmployeeViewModel = this.autofacScope.Resolve<ExpenseDocItemsEmployeeViewModel>(parameter);
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		#endregion


		private void FillUnderreceived()
		{
			Entity.Employee.FillWearInStockInfo(UoW, Entity.Warehouse, Entity.Date, onlyUnderreceived: true);

			foreach(var item in Entity.Employee.UnderreceivedItems) {
				if(item.InStockState != StockStateInfo.Enough && item.InStockState != StockStateInfo.NotEnough) {
					logger.Warn($"На складе отсутствуют позиции для {item.Item.Name}. Пропускаем.");
					continue;
				}
				var stockBalance = item.BestChoiceInStock.First();
				var amount = item.InStockState == StockStateInfo.Enough ? item.NeededAmount : stockBalance.Amount;
				Entity.AddItem(stockBalance.StockPosition, amount);
			}
		}

		public override bool Save()
		{
			logger.Info("Запись документа...");
			var valid = new QSValidator<Expense>(UoWGeneric.Root);

			var ask = new GtkQuestionDialogsInteractive();
			Entity.UpdateOperations(UoW, ask);
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);
			UoWGeneric.Save();
			if(Entity.Operation == ExpenseOperations.Employee) {
				logger.Debug("Обновляем записи о выданной одежде в карточке сотрудника...");
				Entity.UpdateEmployeeNextIssue();
				UoWGeneric.Commit();
			}

			logger.Info("Ok");
			return true;
		}

		private void IssuanceSheetOpen()
		{
			Save();
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		private void Print()
		{
			Save();
			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.Id),
				Identifier = "Statements.IssuanceSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		public void OpenIssuenceSheet()
		{
			if(UoW.HasChanges) {
				if(MessageDialogHelper.RunQuestionDialog("Сохранить документ выдачи перед открытием ведомости?"))
					Save();
				else
					return;
			}
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public void CreateIssuenceSheet()
		{
			Entity.CreateIssuanceSheet();
		}

		public void PrintIssuenceSheet()
		{
			if(UoW.HasChanges) {
				if(QSOrmProject.CommonDialogs.SaveBeforePrint(Entity.GetType(), "ведомости"))
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
			MainClass.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
	}
}
