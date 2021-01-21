using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
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
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QSReport;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Measurements;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Statements;

namespace workwear.ViewModels.Stock
{
	public class MassExpenseViewModel : EntityDialogViewModelBase<MassExpense>
	{
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public ILifetimeScope AutofacScope;
		private readonly IInteractiveService interactive;
		private readonly SizeService sizeService;
		private readonly CommonMessages messages;

		private string displayMessage;
		public virtual string DisplayMessage {
			get { return displayMessage; }
			set { SetField(ref displayMessage, value); }
		}

		public MassExpenseViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiCompatibilityNavigation navigationManager, 
			ILifetimeScope autofacScope, 
			IInteractiveService interactive, 
			IUserService userService,
			StockRepository stockRepository,
			FeaturesService featutesService,
			SizeService sizeService,
			CommonMessages messages,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigationManager, validator)
		{
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.messages = messages ?? throw new ArgumentNullException(nameof(messages));
			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);

			Entity.SizeService = sizeService;
			var entryBuilder = new CommonEEVMBuilderFactory<MassExpense>(this, Entity, UoW, navigationManager) {
				AutofacScope = AutofacScope
			};

			WarehouseFromEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseFrom)
										 .UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
										 .UseViewModelDialog<WarehouseViewModel>()
										 .Finish();

			Entity.ObservableItemsNomenclature.ListContentChanged += ObservableItemsNomenclature_ListContentChanged;
			Entity.ObservableEmployeeCard.ListContentChanged += ObservableItemsNomenclature_ListContentChanged;
			ValidateNomenclature();

			if(Entity.WarehouseFrom == null)
				Entity.WarehouseFrom = stockRepository.GetDefaultWarehouse(UoW, featutesService, userService.CurrentUserId);
		}

		void ObservableItemsNomenclature_ListContentChanged(object sender, EventArgs e)
		{
			ValidateNomenclature();
		}

		void ValidateNomenclature()
		{
			DisplayMessage = $"<span foreground=\"red\">{Entity.ValidateNomenclature(UoW)}</span>";
		}

		#region Size

		public string[] GetSizes(string code) => sizeService.GetSizesForEmployee(code);

		public string[] GetGrowths(Sex sex) => sizeService.GetSizesForEmployee(sex == Sex.F ? GrowthStandartWear.Women : GrowthStandartWear.Men);

		#endregion

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
			foreach(var nom in noms) 
				Entity.ObservableItemsNomenclature.Remove(nom);
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
			if (!Entity.ListEmployees.Any(x=> x.EmployeeCard.FirstName == "-" && x.EmployeeCard.LastName == "-"))
				Entity.AddEmployee(newEmployee, interactive);
		}

		public void RemoveEmployee(MassExpenseEmployee[] employees)
		{
			foreach(var emp in employees) {
				Entity.ObservableEmployeeCard.Remove(Entity.ListEmployees.First(x => x == emp));
			}
		}

		#endregion

		protected override bool Validate()
		{
			foreach(var emp in Entity.ListEmployees) {
				emp.EmployeeCard.FirstName.Replace("-", "");
				emp.EmployeeCard.LastName.Replace("-", "");
				emp.EmployeeCard.Patronymic.Replace("-", "");
			}

			var valid = base.Validate();
			if(!valid)
				return valid;


			Entity.UpdateOperations(UoW, null);
			Entity.UpdateIssuanceSheet();
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
				if(interactive.Question("Сохранить документ выдачи перед открытием ведомости?")) {
					if(!Save())
						return;
				}
				else
					return;
			}
			Entity.UpdateIssuanceSheet();
			NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public void PrintIssuenceSheet(IssuedSheetPrint doc)
		{
			if(UoW.HasChanges) {
				if(messages.SaveBeforePrint(Entity.GetType(), "ведомости"))
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
	}
}
