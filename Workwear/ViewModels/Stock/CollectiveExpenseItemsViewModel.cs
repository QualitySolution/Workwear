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
using QS.Utilities.Debug;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Company;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Stock.Widgets;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Stock
{
	public class CollectiveExpenseItemsViewModel : ViewModelBase
	{
		public readonly CollectiveExpenseViewModel сollectiveExpenseViewModel;
		public readonly FeaturesService featuresService;
		private readonly INavigationManager navigation;
		private readonly IInteractiveMessage interactive;
		private readonly EmployeeRepository employeeRepository;
		private readonly EmployeeIssueModel issueModel;
		private readonly StockBalanceModel stockBalanceModel;
		public SizeService SizeService { get; }
		public BaseParameters BaseParameters { get; }

		public CollectiveExpenseItemsViewModel(
			CollectiveExpenseViewModel collectiveExpenseViewModel,
			FeaturesService featuresService,
			INavigationManager navigation,
			SizeService sizeService,
			EmployeeIssueModel issueModel,
			StockBalanceModel stockBalanceModel,
			EmployeeRepository employeeRepository,
			IInteractiveMessage interactive,
			BaseParameters baseParameters,
			PerformanceHelper performance
			)
		{
			this.сollectiveExpenseViewModel = collectiveExpenseViewModel ?? throw new ArgumentNullException(nameof(collectiveExpenseViewModel));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			
			//Запрашиваем все размеры чтобы были в кеше Uow.
			performance.CheckPoint("Запрашиваем все размеры чтобы были в кеше Uow.");
			SizeService.RefreshSizes(UoW);
			performance.CheckPoint("Загружаем строки документа");
			var allOwners = UoW.Session.QueryOver<Owner>().Future();
			
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			UoW.Session.QueryOver<CollectiveExpense>()
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Skip, x => x.IssuanceSheet)
				.Where(x => x.Id == Entity.Id)
				.JoinAlias(x => x.Items, () => collectiveExpenseItemAlias)
				.Fetch(SelectMode.Fetch, x => x.Items)
				.Fetch(SelectMode.Fetch, () => collectiveExpenseItemAlias.IssuanceSheetItem)
				.Fetch(SelectMode.Fetch, () => collectiveExpenseItemAlias.WarehouseOperation)
				.List();
			
			performance.CheckPoint("Загружаем информацию о сотрудниках");
			var employeeIds = Entity.Items.Select(x => x.Employee.Id).Distinct().ToArray();
			issueModel.PreloadEmployeeInfo(employeeIds);
			
			performance.CheckPoint(nameof(issueModel.PreloadWearItems));
			issueModel.PreloadWearItems(employeeIds);
			
			performance.CheckPoint(nameof(this.issueModel.FillWearInStockInfo));
			var employees = Entity.Items.Select(x => x.Employee).Distinct();
			var excludeOperations = Entity.Items.Select(x => x.WarehouseOperation);
			stockBalanceModel.Warehouse = Entity.Warehouse;
			stockBalanceModel.OnDate = Entity.Date;
			stockBalanceModel.ExcludeOperations = excludeOperations;
			issueModel.FillWearInStockInfo(employees, stockBalanceModel);
			
			performance.CheckPoint("Fill EmployeeCardItem's");
			foreach(var docItem in Entity.Items) {
				docItem.EmployeeCardItem = docItem.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.IsSame(docItem.ProtectionTools));
			}
			
			performance.CheckPoint(nameof(issueModel.FillWearReceivedInfo));
			issueModel.FillWearReceivedInfo(Entity.Employees.ToArray());

			performance.CheckPoint("Finish");
			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.ObservableItems.ListContentChanged += ExpenseDoc_ObservableItems_ListContentChanged;
			Owners = allOwners.ToList();
		}

		#region Хелперы
		private IUnitOfWork UoW => сollectiveExpenseViewModel.UoW;
		public CollectiveExpense Entity => сollectiveExpenseViewModel.Entity;
		#endregion

		#region Поля
		public string Sum => $"Строк в документе: <u>{Entity.Items.Count}</u>" +
				$" Сотрудников: <u>{Entity.Items.Select(x => x.Employee.Id).Distinct().Count()}</u>" +
				$" Единиц продукции: <u>{Entity.Items.Sum(x => x.Amount)}</u>";

		private CollectiveExpenseItem selectedItem;
		[PropertyChangedAlso(nameof(SensitiveButtonDel))]
		[PropertyChangedAlso(nameof(SensitiveButtonChange))]
		[PropertyChangedAlso(nameof(SensitiveRefreshMenuItem))]
		public virtual CollectiveExpenseItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}
		
		public IList<Owner> Owners { get; }

		#endregion
		#region Sensetive
		public bool SensitiveAddButton => Entity.Warehouse != null;
		public bool SensitiveButtonChange => SelectedItem != null;
		public bool SensitiveButtonDel => SelectedItem != null;
		public bool SensitiveRefreshMenuItem => SelectedItem != null;
		public bool SensitiveRefreshAllMenuItem => Entity.Items.Any();
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
		public void AddSubdivisions() {
			var selectJournal = navigation.OpenViewModel<SubdivisionJournalViewModel>(сollectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadSubdivisions;
		}
		public void AddDepartments() {
			var selectJournal = navigation.OpenViewModel<DepartmentJournalViewModel>(сollectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadDepartments;
		}

		private void LoadEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);
			progressPage.ViewModel.Progress.Start(5, text: "Загружаем сотрудников");
			var employeeIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id).ToArray();
			var employees = UoW.Query<EmployeeCard>().Where(x => x.Id.IsIn(employeeIds)).List();
			progressPage.ViewModel.Progress.Add();
			
			AddEmployeesList(employees, progressPage);
		}
		
		private void LoadSubdivisions(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);
			progressPage.ViewModel.Progress.Start(5, text: "Загружаем сотрудников");
			var subdivisionIds = e.GetSelectedObjects<SubdivisionJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromSubdivisions(UoW, subdivisionIds);
			progressPage.ViewModel.Progress.Add();
			
			AddEmployeesList(employees, progressPage);
		}
		
		private void LoadDepartments(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);
			progressPage.ViewModel.Progress.Start(4, text: "Загружаем список сотрудников");
			var departmentsIds = e.GetSelectedObjects<DepartmentJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromDepartments(UoW, departmentsIds);
			
			AddEmployeesList(employees, progressPage);
		}
		
		private void AddEmployeesList(IList<EmployeeCard> employees, IPage<ProgressWindowViewModel> progressPage = null) {
			progressPage?.ViewModel.Progress.Add(text:"Загружаем потребности");
			issueModel.FillWearReceivedInfo(employees.ToArray());
			
			progressPage?.ViewModel.Progress.Add(text:"Загружаем складские остатки");
			issueModel.FillWearInStockInfo(employees, stockBalanceModel);
			progressPage?.ViewModel.Progress.Add();
			if(progressPage != null)
				navigation.ForceClosePage(progressPage, CloseSource.FromParentPage);
			
			//Подготавливаем виджет
			Dictionary<int, IssueWidgetItem> wigetList = new Dictionary<int, IssueWidgetItem>();
			
			var needs = employees
				.SelectMany(x => x.WorkwearItems)
				.Where(x=> !Entity.Items.Any(y =>y.EmployeeCardItem == x))
				.ToList();
			
			foreach(var item in needs) {
				if(wigetList.ContainsKey(item.ProtectionTools.Id)) {
					wigetList[item.ProtectionTools.Id].NumberOfNeeds++;
					wigetList[item.ProtectionTools.Id].ItemQuantityForIssuse += item.CalculateRequiredIssue(BaseParameters, Entity.Date);
					if(item.CalculateRequiredIssue(BaseParameters, Entity.Date) != 0)
						wigetList[item.ProtectionTools.Id].NumberOfCurrentNeeds++;
				}
				else
					wigetList.Add(item.ProtectionTools.Id, new IssueWidgetItem(item.ProtectionTools,
						item.ProtectionTools.Type.IssueType == IssueType.Collective,
						item.CalculateRequiredIssue(BaseParameters, Entity.Date)>0 ? 1 : 0,
						1,
						item.CalculateRequiredIssue(BaseParameters, Entity.Date),
						stockBalanceModel.ForNomenclature(item.ProtectionTools.Nomenclatures.ToArray())
								.Sum(x =>x.Amount)));
			}

			if(!wigetList.Any()) {
				interactive.ShowMessage(ImportanceLevel.Info, "Нет потребностей для добавления");
				return;
			}

			var page = navigation.OpenViewModel<IssueWidgetViewModel, Dictionary<int, IssueWidgetItem>>
				(null, wigetList.OrderByDescending(x => x.Value.Active).ThenBy(x=>x.Value.ProtectionTools.Name)
					.ToDictionary(x => x.Key, x => x.Value));
			
			page.ViewModel.AddItems = (dic,vac) => AddItemsFromWidget(dic, needs, page, vac);
		}

		public void AddItemsFromWidget(Dictionary<int, IssueWidgetItem> widgetItems, List<EmployeeCardItem> needs,
			IPage page, bool excludeOnVaction) {
			foreach(var item in needs) {
				if(widgetItems.First(x => x.Key == item.ProtectionTools.Id).Value.Active)
					if(excludeOnVaction && item.EmployeeCard.OnVacation(Entity.Date))
						continue;
					else
						Entity.AddItem(item, BaseParameters);
			}
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
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) 
				foreach(var item in (List<CollectiveExpenseItem>)page.Tag) 
					item.StockPosition = node.GetStockPosition(UoW);

			OnPropertyChanged(nameof(Sum));
		}

		public void DeleteItem(CollectiveExpenseItem deleteItem)
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
			OnPropertyChanged(nameof(Sum));
		}

		public void DeleteEmployee(CollectiveExpenseItem item)
		{
			var toDelete = Entity.Items.Where(x => x.Employee.IsSame(item.Employee)).ToList();
			foreach(var deleteItem in  toDelete) {
				DeleteItem(deleteItem);
			}
		}
		#endregion
		#region Расчет для View
		public string GetRowColor(CollectiveExpenseItem item) {
			var requiredIssue = item.EmployeeCardItem?.CalculateRequiredIssue(BaseParameters, Entity.Date);
			if(requiredIssue > 0 && item.Nomenclature == null)
				return item.Amount == 0 ? "red" : "Dark red";
			if(requiredIssue > 0 && item.Amount == 0)
				return "blue";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			return null;
		}

		#endregion
		#region Обновление документа

		public void Refresh(CollectiveExpenseItem[] selectedCollectiveExpenseItem) {
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);
			progressPage.ViewModel.Progress.Start(5, text: "Загружаем...");
			progressPage.ViewModel.Progress.Add();
			AddEmployeesList(selectedCollectiveExpenseItem?.Select(x => x.Employee).Distinct().ToList(),progressPage);
			
			Entity.ResortItems();
		}
		public void RefreshAll() {
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);
			progressPage.ViewModel.Progress.Start(5, text: "Загружаем...");
			progressPage.ViewModel.Progress.Add();
			AddEmployeesList(Entity.ObservableItems.Select(x => x.Employee).Distinct().ToList(),progressPage);
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

		private void ExpenseDoc_ObservableItems_ListContentChanged(object sender, EventArgs e) {
			OnPropertyChanged(nameof(SensitiveRefreshAllMenuItem));
			OnPropertyChanged(nameof(Sum));
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
				case nameof(Entity.Warehouse):
					stockBalanceModel.Warehouse = Entity.Warehouse;
					OnPropertyChanged(nameof(SensitiveAddButton));
					break;
				case nameof(Entity.Date):
					stockBalanceModel.OnDate = Entity.Date;
					break;
			}
		}
	}
}
