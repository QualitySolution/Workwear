using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
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

		public ITdiCompatibilityNavigation navigationManager;

		public IssuanceSheetViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ITdiTab myTab, ITdiCompatibilityNavigation navigationManager, ILifetimeScope autofacScope) : base(uowBuilder, unitOfWorkFactory, myTab, navigationManager)
		{
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));

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
													 .UseOrmReferenceJournalAndAutocompleter()
													 .UseTdiEntityDialog()
													 .Finish();

			HeadOfDivisionPersonEntryViewModel = entryBuilder.ForProperty(x => x.HeadOfDivisionPerson)
													 .UseOrmReferenceJournalAndAutocompleter()
													 .UseTdiEntityDialog()
													 .Finish();

			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		#region Таблица 

		public void AddItems()
		{
			var selectPage = navigationManager.OpenTdiTab<OrmReference, Type>(TdiTab, typeof(Nomenclature), OpenPageOptions.AsSlave);

			var selectDialog = selectPage.TdiTab as OrmReference;
			selectDialog.Mode = OrmReferenceMode.MultiSelect;
			selectDialog.ObjectSelected += NomenclatureJournal_ObjectSelected;
		}

		void NomenclatureJournal_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var nomenclature in e.GetEntities<Nomenclature>()) {
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
			var selectPage = navigationManager.OpenTdiTab<ReferenceRepresentation>(
				TdiTab, 
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
			var selectPage = navigationManager.OpenTdiTab<OrmReference, Type>(TdiTab, typeof(Nomenclature), OpenPageOptions.AsSlave);

			var selectDialog = selectPage.TdiTab as OrmReference;
			selectDialog.Tag = items;
			selectDialog.Mode = OrmReferenceMode.Select;
			selectDialog.ObjectSelected += SetNomenclature_ObjectSelected;
		}

		void SetNomenclature_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var items = (sender as OrmReference).Tag as IssuanceSheetItem[];
			foreach(var item in items) {
				item.Nomenclature = e.Subject as Nomenclature;
			}
		}

		#endregion

		#region Кнопки

		public void Print()
		{
			var reportInfo = new ReportInfo {
				Title = String.Format("Ведомость №{0} (МБ-7)", Entity.Id),
				Identifier = "Statements.IssuanceSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
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

	}
}
