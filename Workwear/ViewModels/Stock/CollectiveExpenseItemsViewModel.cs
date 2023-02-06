using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;
using Workwear.Measurements;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock
{
	public class CollectiveExpenseItemsViewModel : ViewModelBase
	{
		public readonly CollectiveExpenseViewModel сollectiveExpenseViewModel;
		public readonly FeaturesService featuresService;
		private readonly INavigationManager navigation;
		private readonly IDeleteEntityService deleteService;

		public SizeService SizeService { get; }
		public BaseParameters BaseParameters { get; }

		public CollectiveExpenseItemsViewModel(CollectiveExpenseViewModel сollectiveExpenseViewModel, FeaturesService featuresService, INavigationManager navigation, SizeService sizeService, IDeleteEntityService deleteService, BaseParameters baseParameters, IProgressBarDisplayable globalProgress)
		{
			this.сollectiveExpenseViewModel = сollectiveExpenseViewModel ?? throw new ArgumentNullException(nameof(сollectiveExpenseViewModel));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));
			BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));

			//Предварительная загрузка элементов для более быстрого открытия документа
			globalProgress.Start(2);
			var query = UoW.Session.QueryOver<CollectiveExpenseItem>()
				.Where(x => x.Document.Id == Entity.Id)
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Skip, x => x.IssuanceSheetItem)
				.Fetch(SelectMode.Fetch, x => x.WarehouseOperation)
				.Future();

			UoW.Session.QueryOver<CollectiveExpenseItem>()
				.Where(x => x.Document.Id == Entity.Id)
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Skip, x => x.IssuanceSheetItem)
				.Fetch(SelectMode.Fetch, x => x.Employee)
				.Fetch(SelectMode.Fetch, x => x.Employee.WorkwearItems)
				.Future();

			query.ToList();
			globalProgress.Add();

			Entity.PrepareItems(UoW, baseParameters);
			globalProgress.Add();

			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.ObservableItems.ListContentChanged += ExpenceDoc_ObservableItems_ListContentChanged;
			OnPropertyChanged(nameof(Sum));
			globalProgress.Close();

			Owners = UoW.GetAll<Owner>().ToList();
			
			savedItems = new List<CollectiveExpenseItem>();
			foreach(var x in Entity.Items) {
				savedItems.Add((CollectiveExpenseItem)x.Clone());
			}
		}

		#region Хелперы
		private IUnitOfWork UoW => сollectiveExpenseViewModel.UoW;
		public CollectiveExpense Entity => сollectiveExpenseViewModel.Entity;
		#endregion

		#region Поля
		public string Sum => $"Строк в документе: <u>{Entity.Items.Count}</u>" +
				$" Сотрудников: <u>{Entity.Items.Select(x => x.Employee.Id).Distinct().Count()}</u>" +
				$" Единиц продукции: <u>{Entity.Items.Sum(x => x.Amount)}</u>";

		public virtual Warehouse Warehouse {
			get { return Entity.Warehouse; }
			set { Entity.Warehouse = value; }
		}

		private CollectiveExpenseItem selectedItem;
		public virtual CollectiveExpenseItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}
		
		public IList<Owner> Owners { get; }

		private IList<CollectiveExpenseItem> savedItems;
		/// <summary>
		/// Копия выдач уже сохранённых в базе
		/// </summary>
		public IList<CollectiveExpenseItem> SavedItems {
			get => savedItems;
		}

		#endregion
		#region Sensetive
		public bool SensetiveAddButton => Entity.Warehouse != null;
		#endregion
		#region Visible
		public bool VisibleSignColumn => featuresService.Available(WorkwearFeature.IdentityCards);
		#endregion
		#region Действия View

		public void AddEmployees(){
			var selectJournal = navigation.OpenViewModel<EmployeeJournalViewModel>(сollectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadEmployees;
		}
		public void AddSubdivizions() {
			var selectJournal = navigation.OpenViewModel<SubdivisionJournalViewModel>(сollectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadSubdivizions;
		}

		private void LoadEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var employeeIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id).ToArray();
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);

			var progress = progressPage.ViewModel.Progress;
			progress.Start(employeeIds.Length * 2 + 1, text: "Загружаем сотрудников");
			var employees = UoW.Query<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.List();
			foreach(var employee in employees) {
				progress.Add(text: employee.ShortName);
				employee.FillWearInStockInfo(UoW, BaseParameters, Entity.Warehouse, Entity.Date);
				progress.Add();
			}
			navigation.ForceClosePage(progressPage, CloseSource.FromParentPage);
			AddEmployeesList(employees);
		}
		
		private void LoadSubdivizions(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var subdivizionIds = e.GetSelectedObjects<SubdivisionJournalNode>().Select(x => x.Id).ToArray();
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);
			

			var employees = UoW.Query<EmployeeCard>()
				.Where(x => x.Subdivision.Id.IsIn(subdivizionIds))
				.List();
			
			var progress = progressPage.ViewModel.Progress;
			progress.Start(employees.Count * 2 + 1, text: "Загружаем сотрудников");
			foreach(var employee in employees) {
				progress.Add(text: employee.ShortName);
				employee.FillWearInStockInfo(UoW, BaseParameters, Entity.Warehouse, Entity.Date);
				progress.Add();
			}
			navigation.ForceClosePage(progressPage, CloseSource.FromParentPage);
			AddEmployeesList(employees);
		}
		
		private void AddEmployeesList(IList<EmployeeCard> employees) {
			List<EmployeeCardItem> needs = employees.SelectMany(x => x.WorkwearItems).ToList();

			var itemsList = Entity.FillListItems(needs, BaseParameters, false);
			Dictionary<int, IssueWidgetItem> wigetList = new Dictionary<int, IssueWidgetItem>();
			foreach(var item in itemsList) {
				if(wigetList.Any(x => x.Key == item.ProtectionTools.Id)) {
					wigetList[item.ProtectionTools.Id].NumberOfNeeds++;
					wigetList[item.ProtectionTools.Id].NumberOfIssused += item.EmployeeCardItem.Amount;
				}
				else
					wigetList.Add(item.ProtectionTools.Id, new IssueWidgetItem(item.ProtectionTools, item.Nomenclature, 1,item.EmployeeCardItem.Amount,
						item.ProtectionTools.Type.IssueType == IssueType.Collective ? true : false));
			}
			
			var page = navigation.OpenViewModel<IssueWidgetViewModel, CollectiveExpenseItemsViewModel, Dictionary<int, IssueWidgetItem>>
				(null, this, wigetList.OrderByDescending(x => x.Value.Active).ToDictionary(x => x.Key, x => x.Value));
			page.ViewModel.AddItems += () => AddItemsAdvanced(itemsList, wigetList, page);
		}
		public void AddItemsAdvanced(List<CollectiveExpenseItem> itemsList, Dictionary<int, IssueWidgetItem> wigetItems, IPage<IssueWidgetViewModel> page) {
			
			for (int i = itemsList.Count - 1; i >= 0; i--) //корректировка в соответствии с данными виджета
				if(!wigetItems.First(x => x.Key == itemsList[i].ProtectionTools.Id)
				   .Value.Active)
					itemsList.RemoveAt(i); 
			
			Entity.AddListItems(itemsList);
			navigation.ForceClosePage(page);
		}

		public void ChangeStockPosition(CollectiveExpenseItem item) {
			ChangeItemPositions(new List<CollectiveExpenseItem>(){item});
		}

		public void ChangeManyStockPositions(CollectiveExpenseItem item) {
			ChangeItemPositions(Entity.Items.Where(x => x.ProtectionTools.Id == item.ProtectionTools.Id).ToList());
		}
		
		private void ChangeItemPositions(List<CollectiveExpenseItem> items)
		{
			var selectJournal = navigation.OpenViewModel<StockBalanceJournalViewModel>(сollectiveExpenseViewModel, QS.Navigation.OpenPageOptions.AsSlave);

			selectJournal.ViewModel.Filter.Warehouse = сollectiveExpenseViewModel.Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.Filter.ProtectionTools = items.First().ProtectionTools;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectJournal.Tag = items;
			selectJournal.ViewModel.OnSelectResult +=SetNomenclatureForRows;
		}

		public void SetNomenclatureForRows(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var page = navigation.FindPage((DialogViewModelBase)sender);
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				foreach(var item in (List<CollectiveExpenseItem>)page.Tag) {
					int a = item.EmployeeCardItem.Amount;
					int b = Entity.AmountInList(node.GetStockPosition(UoW), Entity.Items);
					int c = Entity.AmountInList(node.GetStockPosition(UoW), SavedItems);
					int d = node.Amount;
					//Сверка потребности с учётом сохранённого документа, добавленных строк и остатков на складе
					if(!Equals(item.StockPosition, node.GetStockPosition(UoW)) && d>=a+b-c) {
						item.StockPosition = node.GetStockPosition(UoW);
						item.Amount = item.EmployeeCardItem.Amount;
					}
				}
			}
			OnPropertyChanged(nameof(Sum));
		}

		public void Delete(CollectiveExpenseItem item)
		{
			DeleteItem(item);
			OnPropertyChanged(nameof(Sum));
		}

		public void DeleteEmployee(CollectiveExpenseItem item)
		{
			var toDelete = Entity.Items.Where(x => x.Employee.IsSame(item.Employee)).ToList();
			foreach(var deleteItem in  toDelete) {
				DeleteItem(deleteItem);
			}

			OnPropertyChanged(nameof(Sum));
		}
		#endregion

		#region Обновление документа

		public void Refresh(CollectiveExpenseItem[] selectedCollectiveExpenseItem = null) {
			if(selectedCollectiveExpenseItem == null) 
				AddEmployeesList(Entity.ObservableItems.Select(x => x.Employee).Distinct().ToList());
			else 
				AddEmployeesList(selectedCollectiveExpenseItem.Select(x => x.Employee).Distinct().ToList());
			Entity.ResortItems();
		}

		#endregion

		#region Открытие
		public void OpenEmployee(CollectiveExpenseItem item)
		{
			navigation.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(сollectiveExpenseViewModel, EntityUoWBuilder.ForOpen(item.Employee.Id));
		}

		public void OpenNomenclature(CollectiveExpenseItem item)
		{
			navigation.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(сollectiveExpenseViewModel, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}

		public void OpenProtectionTools(CollectiveExpenseItem item)
		{
			navigation.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(сollectiveExpenseViewModel, EntityUoWBuilder.ForOpen(item.ProtectionTools.Id));
		}
		#endregion

		private void ExpenceDoc_ObservableItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Sum));
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(nameof(Entity.Warehouse) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveAddButton));
		}
		private void DeleteItem(CollectiveExpenseItem deleteItem)
		{
			if(deleteItem.Id > 0) {
				UoW.Delete(deleteItem);
			}
			if(deleteItem.IssuanceSheetItem != null) {
				if(deleteItem.IssuanceSheetItem.Id > 0)
					UoW.Delete(deleteItem);
				Entity.IssuanceSheet.Items.Remove(deleteItem.IssuanceSheetItem);
			}
			Entity.RemoveItem(deleteItem);
		}
	}
}
