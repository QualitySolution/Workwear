using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
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
using QSReport;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.Domain.Users;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Company;
using workwear.ViewModels.Statements;

namespace workwear.ViewModels.Stock
{
	public class ExpenseEmployeeViewModel : EntityDialogViewModelBase<Expense>
	{
		ILifetimeScope autofacScope;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public ExpenseDocItemsEmployeeViewModel DocItemsEmployeeViewModel;
		IInteractiveQuestion interactive;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;

		public ExpenseEmployeeViewModel(IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IValidator validator,
			IUserService userService,
			IInteractiveQuestion interactive,
			StockRepository stockRepository,
			CommonMessages commonMessages,
			FeaturesService featuresService,
			EmployeeCard employee = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.interactive = interactive;
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			var entryBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, navigation, autofacScope);
			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
				Entity.Operation = ExpenseOperations.Employee;
			}

			if(employee != null) {
				Entity.Employee = UoW.GetById<EmployeeCard>(employee.Id);
				Entity.Warehouse = Entity.Employee.Subdivision?.Warehouse;
			}

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			if(employee != null)
				FillUnderreceived();

			if(Entity.WriteOffDoc != null)
				FillAktNumber();

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
			Entity.PropertyChanged += EntityChange;
		}

		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		#endregion

		private void FillUnderreceived()
		{
			Entity.ObservableItems.Clear();

			Entity.Employee.FillWearInStockInfo(UoW, Entity.Warehouse, Entity.Date, onlyUnderreceived: false);

			var r = Entity.Employee;
			if (Entity.Employee.WorkwearItems.Count > 0)
			foreach(var item in Entity.Employee.WorkwearItems) {
				Entity.AddItem(item);
			}
		}

		public void FillAktNumber()
		{
			foreach(var item in Entity.WriteOffDoc.Items)
				foreach(var i in Entity.Items)
					if(item.Nomenclature == i.Nomenclature)
						i.AktNumber = item.AktNumber;
		}

		public override bool Save()
		{
			if(!Validate())
				return false;

			logger.Info("Запись документа...");

			if(Entity.Items.FirstOrDefault(x => x.IsWriteOff) != null && Entity.WriteOffDoc == null) {
				Entity.WriteOffDoc = new Writeoff();
				Entity.WriteOffDoc.Date = Entity.Date;
				Entity.WriteOffDoc.CreatedbyUser = Entity.CreatedbyUser;
			}

			Entity.CleanupItems();
			Entity.CleanupItemsWriteOff();
			Entity.UpdateOperations(UoW, interactive);
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);



			Entity.UpdateIssuedWriteOffOperation();

			if(Entity.WriteOffDoc != null)
				UoW.Save(Entity.WriteOffDoc);

			UoWGeneric.Save();
			if(Entity.Operation == ExpenseOperations.Employee) {
				logger.Debug("Обновляем записи о выданной одежде в карточке сотрудника...");
				Entity.UpdateEmployeeWearItems();
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

		public void PrintIssuenceSheet(IssuedSheetPrint doc)
		{
			if(UoW.HasChanges) {
				if(commonMessages.SaveBeforePrint(Entity.GetType(), "ведомости"))
					Save();
				else
					return;
			}

			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.IssuanceSheet.Id),
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id }
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		public void EntityChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Employee)) {
				FillUnderreceived();
			}
		}

	}
}
