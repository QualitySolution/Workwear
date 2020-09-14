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
using QS.Report;
using QS.Report.ViewModels;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QSReport;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Tools.Oracle;
using workwear.ViewModels.Company;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Statements
{
	public class IssuanceSheetViewModel : LegacyEntityDialogViewModelBase<IssuanceSheet>
	{
		public EntityEntryViewModel<Organization> OrganizationEntryViewModel;
		public EntityEntryViewModel<Subdivision> SubdivisionEntryViewModel;
		public EntityEntryViewModel<Leader> ResponsiblePersonEntryViewModel;
		public EntityEntryViewModel<Leader> HeadOfDivisionPersonEntryViewModel;
		public ILifetimeScope AutofacScope;
		public ITdiCompatibilityNavigation tdiNavigationManager;
		private readonly CommonMessages commonMessages;
		private readonly HRSystem hRSystem;

		public IssuanceSheetViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ITdiTab myTab, ITdiCompatibilityNavigation navigationManager, IValidator validator, ILifetimeScope autofacScope, CommonMessages commonMessages, HRSystem hRSystem) : base(uowBuilder, unitOfWorkFactory, myTab, navigationManager, validator)
		{
			this.tdiNavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.commonMessages = commonMessages;
			this.hRSystem = hRSystem ?? throw new ArgumentNullException(nameof(hRSystem));
			var entryBuilder = new LegacyEEVMBuilderFactory<IssuanceSheet>(this, TdiTab, Entity, UoW, navigationManager) {
				AutofacScope = AutofacScope
			};

			OrganizationEntryViewModel = entryBuilder.ForProperty(x => x.Organization)
													 .UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
													 .UseViewModelDialog<OrganizationViewModel>()
													 .Finish();

			SubdivisionEntryViewModel = entryBuilder.ForProperty(x => x.Subdivision)
													 .UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
													 .UseViewModelDialog<SubdivisionViewModel>()
										 			 .Finish();

			ResponsiblePersonEntryViewModel = entryBuilder.ForProperty(x => x.ResponsiblePerson)
													.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
													.UseViewModelDialog<LeadersViewModel>()
													.Finish();


			HeadOfDivisionPersonEntryViewModel = entryBuilder.ForProperty(x => x.HeadOfDivisionPerson)
													.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
													.UseViewModelDialog<LeadersViewModel>()
													.Finish();


			Entity.PropertyChanged += Entity_PropertyChanged;

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<ExpenseItem>(Expense_Changed);
		}

		#region Таблица 

		public void AddItems()
		{
			var selectPage = NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += NomenclatureJournal_OnSelectResult;
		}

		void NomenclatureJournal_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var nomeclatures = UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()));
			foreach(var nomenclature in nomeclatures) {
				var item = new IssuanceSheetItem {
					IssuanceSheet = Entity,
					Nomenclature = nomenclature,
					StartOfUse = Entity.Date,
					Amount = 1,
					Lifetime = 12,
				};
				Entity.ObservableItems.Add(item);
			}
		}

		public void RemoveItems(IssuanceSheetItem[] items)
		{
			foreach(var item in items) {
				Entity.ObservableItems.Remove(item);
			}
		}

		public void SetEmployee(IssuanceSheetItem[] items)
		{
			var selectPage = NavigationManager.OpenViewModel<EmployeeJournalViewModel>(
				this,
				OpenPageOptions.AsSlave);

			var selectDialog = selectPage.ViewModel;
			selectDialog.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectDialog.Tag = items;
			selectDialog.OnSelectResult += SelectDialog_OnSelectResult;
		}

		void SelectDialog_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var items = (sender as EmployeeJournalViewModel).Tag as IssuanceSheetItem[];
			var employee = UoW.GetById<EmployeeCard>(DomainHelper.GetId(e.SelectedObjects.First()));
			foreach(var item in items) {
				item.Employee = employee;
			}
		}

		public void SetNomenclature(IssuanceSheetItem[] items)
		{
			var selectPage = NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectPage.Tag = items;
			selectPage.ViewModel.OnSelectResult += SetNomenclature_OnSelectResult;
		}

		void SetNomenclature_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var page = NavigationManager.FindPage((DialogViewModelBase)sender);
			var items = page.Tag as IssuanceSheetItem[];
			var nomenclature = UoW.GetById<Nomenclature>(e.SelectedObjects[0].GetId());
			foreach(var item in items) {
				item.Nomenclature = nomenclature;
			}
		}

		public void OpenNomenclature(Nomenclature nomenclature)
		{
			NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(nomenclature.Id));
		}

		#endregion

		#region Sensetive

		public bool CanEditItems => Entity.Expense == null;

		#endregion

		#region Visible

		public bool VisibleExpense => Entity.Expense != null;
		public bool VisibleFillBy => Entity.Expense == null;
		public bool VisibleCloseFillBy => FillByViewModel != null;

		#endregion

		#region Кнопки

		public void Print(IssuedSheetPrint doc)
		{
			if(UoW.HasChanges) {
				if(commonMessages.SaveBeforePrint(Entity.GetType(), "ведомости"))
					Save();
				else
					return;
			}
			var subdivisionHR = hRSystem.GetSubdivision(Entity.Items.First().Employee.SubdivisionId.Value);

			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.Id),
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id },
					{"organization", subdivisionHR.Name },
					{"code", subdivisionHR.Code}
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion

		#region Выдача сотруднику

		public void OpenExpense()
		{
			if (Entity.Expense != null)
				tdiNavigationManager.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.Expense.Id));
			else
				tdiNavigationManager.OpenViewModel<WarehouseMassExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.MassExpense.Id));

		}

		void Expense_Changed(EntityChangeEvent[] changeEvents)
		{
			if(changeEvents.Any(x => (x.Entity as ExpenseItem).ExpenseDoc.Id == Entity.Expense?.Id)){
				Entity.ReloadChildCollection(x => x.Items, x => x.IssuanceSheet, UoW.Session);
				Entity.CleanObservableItems();
			}
		}

		#endregion

		#region Заполнение таблицы

		private ViewModelBase fillByViewModel;
		[PropertyChangedAlso(nameof(VisibleCloseFillBy))]
		public virtual ViewModelBase FillByViewModel {
			get => fillByViewModel;
			set => SetField(ref fillByViewModel, value);
		}

		public void OpenFillBy()
		{
			FillByViewModel = AutofacScope.Resolve<IssuanceSheetFillByViewModel>(
				new TypedParameter(typeof(IssuanceSheetViewModel), this)
			);
		}

		public void CloseFillBy()
		{
			FillByViewModel = null;
		}

		#endregion

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(Entity.Date):
					foreach(var item in Entity.Items) {
						item.StartOfUse = Entity.Date;
					}
					break;
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}

	}
}
