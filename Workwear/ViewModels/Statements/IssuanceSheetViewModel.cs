using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSReport;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.JournalViewModels.Company;
using workwear.Representations.Organization;
using workwear.ViewModels.Company;

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

		public IssuanceSheetViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ITdiTab myTab, ITdiCompatibilityNavigation navigationManager, IValidator validator, ILifetimeScope autofacScope, CommonMessages commonMessages) : base(uowBuilder, unitOfWorkFactory, myTab, navigationManager, validator)
		{
			this.tdiNavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.commonMessages = commonMessages;
			var entryBuilder = new LegacyEEVMBuilderFactory<IssuanceSheet>(this, TdiTab, Entity, UoW, navigationManager) {
				AutofacScope = AutofacScope
			};

			OrganizationEntryViewModel = entryBuilder.ForProperty(x => x.Organization)
													 .UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
													 .UseViewModelDialog<OrganizationViewModel>()
													 .Finish();

			SubdivisionEntryViewModel = entryBuilder.ForProperty(x => x.Subdivision)
													 .UseOrmReferenceJournalAndAutocompleter()
													 .UseTdiEntityDialog()
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
			var selectPage = tdiNavigationManager.OpenTdiTab<OrmReference, Type>(this, typeof(Nomenclature), OpenPageOptions.AsSlave);

			var selectDialog = selectPage.TdiTab as OrmReference;
			selectDialog.Mode = OrmReferenceMode.MultiSelect;
			selectDialog.ObjectSelected += NomenclatureJournal_ObjectSelected;
		}

		void NomenclatureJournal_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var ids = e.GetEntities<Nomenclature>().Select(x => x.Id);
			var nomenclatures = UoW.GetById<Nomenclature>(ids);
			foreach(var nomenclature in nomenclatures) {
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
			var selectPage = tdiNavigationManager.OpenTdiTab<ReferenceRepresentation>(
				this, 
				OpenPageOptions.AsSlave, 
				c => c.RegisterType<EmployeesVM>().As<IRepresentationModel>()
			);

			var selectDialog = selectPage.TdiTab as ReferenceRepresentation;
			selectDialog.Mode = OrmReferenceMode.Select;
			selectDialog.Tag = items;
			selectDialog.ObjectSelected += SelectEmployee_ObjectSelected;
		}

		void SelectEmployee_ObjectSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var items = (sender as ReferenceRepresentation).Tag as IssuanceSheetItem[];
			var employee = UoW.GetById<EmployeeCard>(e.ObjectId);
			foreach(var item in items) {
				item.Employee = employee;
			}
		}

		public void SetNomenclature(IssuanceSheetItem[] items)
		{
			var selectPage = tdiNavigationManager.OpenTdiTab<OrmReference, Type>(this, typeof(Nomenclature), OpenPageOptions.AsSlave);

			var selectDialog = selectPage.TdiTab as OrmReference;
			selectDialog.Tag = items;
			selectDialog.Mode = OrmReferenceMode.Select;
			selectDialog.ObjectSelected += SetNomenclature_ObjectSelected;
		}

		void SetNomenclature_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var items = (sender as OrmReference).Tag as IssuanceSheetItem[];
			foreach(var item in items) {
				item.Nomenclature = UoW.GetById<Nomenclature>(e.Subject.GetId());
			}
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

			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.Id),
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion

		#region Выдача сотруднику

		public void OpenExpense()
		{
			tdiNavigationManager.OpenTdiTab<ExpenseDocDlg, Expense>(this, Entity.Expense);
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
