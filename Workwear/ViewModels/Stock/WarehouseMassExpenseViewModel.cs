using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.ViewModels.Statements;

namespace workwear.ViewModels.Stock
{
	public class WarehouseMassExpenseViewModel : LegacyEntityDialogViewModelBase<MassExpense>
	{
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public ILifetimeScope AutofacScope;
		private readonly IInteractiveService interactive;
		private readonly CommonMessages messages;

		private string displayMessage;
		public virtual string DisplayMessage {
			get { return displayMessage; }
			set { SetField(ref displayMessage, value); }
		}

		public WarehouseMassExpenseViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiTab myTab, 
			ITdiCompatibilityNavigation navigationManager, 
			ILifetimeScope autofacScope, 
			IInteractiveService interactive, 
			IUserService userService,
			CommonMessages messages,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, myTab, navigationManager, validator)
		{
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.messages = messages ?? throw new ArgumentNullException(nameof(messages));
			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);

			var entryBuilder = new LegacyEEVMBuilderFactory<MassExpense>(this, TdiTab, Entity, UoW, navigationManager) {
				AutofacScope = AutofacScope
			};

			WarehouseFromEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseFrom)
										 .UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
										 .UseViewModelDialog<WarehouseViewModel>()
										 .Finish();

			Entity.ObservableItemsNomenclature.ListContentChanged += ObservableItemsNomenclature_ListContentChanged;
			Entity.ObservableEmployeeCard.ListContentChanged += ObservableItemsNomenclature_ListContentChanged;
			ValidateNomenclature();
		}

		void ObservableItemsNomenclature_ListContentChanged(object sender, EventArgs e)
		{
			ValidateNomenclature();
		}

		void ValidateNomenclature()
		{
			DisplayMessage = $"<span foreground=\"red\">{Entity.ValidateNomenclature(UoW)}</span>";
		}

		#region Nomenclature
		public void AddNomenclature()
		{
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceShortSummaryJournalViewModel>(this, QS.Navigation.OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.Warehouse = Entity.WarehouseFrom;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += Nomeclature_OnSelectResult;
		}

		private void Nomeclature_OnSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var nomenclatures = UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()));
			foreach(var nomenclature in nomenclatures) {
				Entity.AddItemNomenclature(nomenclature, interactive, UoW);
			}
		}

		public void RemoveNomenclature(MassExpenseNomenclature[] noms)
		{
			foreach(var nom in noms) {
				Entity.ObservableItemsNomenclature.Remove(nom);
			}
		}

		#endregion

		#region Employee
		public void AddEmployee()
		{
			var selectPage = NavigationManager.OpenViewModel<EmployeeJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += Employee_OnSelectResult;
		}

		private void Employee_OnSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var employees = UoW.GetById<EmployeeCard>(e.SelectedObjects.Select(x => x.GetId()));
			foreach(var emp in employees) {
				Entity.AddEmployee(emp, interactive);
			}
		}

		public void AddNewEmployee()
		{
			var newEmployee = new EmployeeCard();
			newEmployee.FirstName = "-";
			newEmployee.LastName = "-";
			newEmployee.Patronymic = "-";
			if (!Entity.Employees.Any(x=> x.EmployeeCard.FirstName == "-" && x.EmployeeCard.LastName == "-"))
				Entity.AddEmployee(newEmployee, interactive);
		}

		public void RemoveEmployee(MassExpenseEmployee[] employees)
		{
			foreach(var emp in employees) {
				Entity.ObservableEmployeeCard.Remove(Entity.Employees.First(x => x == emp));
			}
		}

		#endregion

		protected override bool Validate()
		{
			foreach(var emp in Entity.Employees) {
				emp.EmployeeCard.FirstName.Replace("-", "");
				emp.EmployeeCard.LastName.Replace("-", "");
				emp.EmployeeCard.Patronymic.Replace("-", "");
			}

			var valid = base.Validate();
			if(!valid)
				return valid;


			Entity.UpdateOperations(UoW, null);
			return true;
		}

		public override void Dispose()
		{
			base.Dispose();
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}

		public void IssuanceSheetCreate()
		{
			Entity.CreateIssuanceSheet();

		}

		public void IssuanceSheetOpen()
		{
			if(UoW.HasChanges) {
				if(interactive.Question("Сохранить документ выдачи перед открытием ведомости?"))
					Save();
				else
					return;
			}
			var t = Entity.IssuanceSheet.Id;

			NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}
		public void IssuanceSheetPrint()
		{
			if(UoW.HasChanges) {
				if(messages.SaveBeforePrint(Entity.GetType(), "ведомости"))
					Save();
				else
					return;
			}

			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.IssuanceSheet.Id),
				Identifier = "Statements.IssuanceSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
	}
}
