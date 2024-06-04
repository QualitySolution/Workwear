using System;
using System.Collections.Generic;
using System.IO;
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
using QS.Services;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Statements
{
	public class IssuanceSheetViewModel : EntityDialogViewModelBase<IssuanceSheet>
	{
		public EntityEntryViewModel<Organization> OrganizationEntryViewModel;
		public EntityEntryViewModel<Subdivision> SubdivisionEntryViewModel;
		public EntityEntryViewModel<EmployeeCard> TransferAgentEntryViewModel;
		public EntityEntryViewModel<Leader> ResponsiblePersonEntryViewModel;
		public EntityEntryViewModel<Leader> HeadOfDivisionPersonEntryViewModel;
		private readonly BaseParameters baseParameters;
		public ILifetimeScope AutofacScope;
		private readonly CommonMessages commonMessages;

		public IssuanceSheetViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigationManager, 
			IValidator validator, 
			ILifetimeScope autofacScope,
			SizeService sizeService,
			CommonMessages commonMessages, BaseParameters baseParameters) : base(uowBuilder, unitOfWorkFactory, navigationManager, validator)
		{
			this.AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.commonMessages = commonMessages;
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			var entryBuilder = new CommonEEVMBuilderFactory<IssuanceSheet>(this, Entity, UoW, navigationManager) {
				AutofacScope = AutofacScope
			};

			OrganizationEntryViewModel = entryBuilder.ForProperty(x => x.Organization).MakeByType().Finish();
			SubdivisionEntryViewModel = entryBuilder.ForProperty(x => x.Subdivision).MakeByType().Finish();
			TransferAgentEntryViewModel = entryBuilder.ForProperty(x => x.TransferAgent).MakeByType().Finish();
			ResponsiblePersonEntryViewModel = entryBuilder.ForProperty(x => x.ResponsiblePerson).MakeByType().Finish();
			HeadOfDivisionPersonEntryViewModel = entryBuilder.ForProperty(x => x.HeadOfDivisionPerson).MakeByType().Finish();
			
			Entity.PropertyChanged += Entity_PropertyChanged;

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<ExpenseItem>(Expense_Changed);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<CollectiveExpenseItem>(CollectiveExpense_Changed);
			if (Entity.Id == 0 )
				GetDefualtSetting();

			if(!UoW.IsNew)
				AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
			
			Entity.Items.ContentChanged += (sender, args) => OnPropertyChanged(nameof(Sum));
		}

		#region Поля View
		public string Sum => $"Строк в документе: <u>{Entity.Items.Count}</u>" +
			$" Сотрудников: <u>{Entity.Items.Where(x => x.Employee != null).Select(x => x.Employee.Id).Distinct().Count()}</u>" +
			$" Единиц продукции: <u>{Entity.Items.Sum(x => x.Amount)}</u>";
		
		public bool SensitiveDocNumber => !AutoDocNumber;
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumber))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public bool AutoDocNumber { get => autoDocNumber; set => SetField(ref autoDocNumber, value); }
		public string DocNumber {
			get => AutoDocNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.DocNumber;
			set => Entity.DocNumber = (AutoDocNumber || value == "авто") ? null : value;
		}
		#endregion

		#region Таблица 

		public SizeService SizeService { get; }

		void GetDefualtSetting()
		{
			var user = AutofacScope.Resolve<IUserService>();
			UserSettings settings = UoW.Session.QueryOver<UserSettings>()
			.Where(x => x.User.Id == user.CurrentUserId).SingleOrDefault<UserSettings>();
			if(settings?.DefaultResponsiblePerson != null)
				Entity.ResponsiblePerson = settings.DefaultResponsiblePerson;
			if(settings?.DefaultLeader != null)
				Entity.HeadOfDivisionPerson = settings.DefaultLeader;
			if(settings?.DefaultOrganization!= null)
				Entity.Organization = settings.DefaultOrganization;
		}
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
				Entity.Items.Add(item);
			}
		}

		public void RemoveItems(IssuanceSheetItem[] items)
		{
			foreach(var item in items) {
				Entity.Items.Remove(item);
			}
		}

		public void SetEmployee(IssuanceSheetItem[] items)
		{
			if(items == null || items.Length == 0)
				return;
			 
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
			var items = (IssuanceSheetItem[])((EmployeeJournalViewModel)sender).Tag;
			var employee = UoW.GetById<EmployeeCard>(e.SelectedObjects.First().GetId());
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

		public bool CanEditItems => Entity.Expense == null && Entity.CollectiveExpense == null;
		public bool CanEditTransferAgent => Entity.CollectiveExpense == null;

		#endregion

		#region Visible

		public bool VisibleExpense => Entity.Expense != null || Entity.CollectiveExpense != null;
		public bool VisibleFillBy => CanEditItems;
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
				Title = doc == IssuedSheetPrint.AssemblyTask ? $"Задание на сборку №{Entity.DocNumber ?? Entity.Id.ToString()}" 
					: $"Ведомость №{Entity.DocNumber ?? Entity.Id.ToString()} (МБ-7)",
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			//Если пользователь не хочет сворачивать ФИО и табельник (настройка в базе)
			if((doc == IssuedSheetPrint.IssuanceSheet || doc == IssuedSheetPrint.IssuanceSheetVertical) && !baseParameters.CollapseDuplicateIssuanceSheet)
				reportInfo.Source = File.ReadAllText(reportInfo.GetPath()).Replace("<HideDuplicates>Data</HideDuplicates>", "<HideDuplicates></HideDuplicates>");

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion

		#region Выдача сотруднику

		public void OpenExpense()
		{
			if(Entity.Expense != null)
				NavigationManager.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(
					this, EntityUoWBuilder.ForOpen(Entity.Expense.Id));
			else if(Entity.CollectiveExpense != null)
				NavigationManager.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(
					this, EntityUoWBuilder.ForOpen(Entity.CollectiveExpense.Id));
			else
				throw new NotSupportedException();

		}

		void Expense_Changed(EntityChangeEvent[] changeEvents)
		{
			if(changeEvents.Any(x => (x.Entity as ExpenseItem).ExpenseDoc.Id == Entity.Expense?.Id)){
				Entity.ReloadChildCollection(x => x.Items, x => x.IssuanceSheet, UoW.Session);
			}
		}

		void CollectiveExpense_Changed(EntityChangeEvent[] changeEvents)
		{
			if(changeEvents.Any(x => (x.Entity as CollectiveExpenseItem).Document.Id == Entity.CollectiveExpense?.Id)) {
				Entity.ReloadChildCollection(x => x.Items, x => x.IssuanceSheet, UoW.Session);
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
