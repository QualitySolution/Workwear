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
using Workwear.Measurements;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Stock.Widgets;
using Workwear.Models.Operations;
using Workwear.Repository.Stock;

namespace Workwear.ViewModels.Stock
{
	public class CollectiveExpenseItemsViewModel : ViewModelBase
	{
		public readonly CollectiveExpenseViewModel сollectiveExpenseViewModel;
		public readonly FeaturesService featuresService;
		private readonly INavigationManager navigation;
		private readonly IInteractiveMessage interactive;
		private readonly EmployeeRepository employeeRepository;
		private readonly StockRepository stockRepository;
		private readonly EmployeeIssueModel issueModel;
		public SizeService SizeService { get; }
		public BaseParameters BaseParameters { get; }

		public CollectiveExpenseItemsViewModel(
			CollectiveExpenseViewModel collectiveExpenseViewModel,
			FeaturesService featuresService,
			INavigationManager navigation,
			SizeService sizeService,
			EmployeeIssueModel issueModel,
			EmployeeRepository employeeRepository,
			IProgressBarDisplayable globalProgress, 
			StockRepository stockRepository,
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
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));

			//Предварительная загрузка элементов для более быстрого открытия документа
			globalProgress.Start(4);
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
			performance.CheckPoint("query");
			globalProgress.Add();
			Entity.PrepareItems(UoW);
			performance.CheckPoint("PrepareItems");
			globalProgress.Add();
			issueModel.FillWearReceivedInfo(Entity.Employees.ToArray());
			performance.CheckPoint("FillWearReceivedInfo");
			globalProgress.Add();

			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.ObservableItems.ListContentChanged += ExpenseDoc_ObservableItems_ListContentChanged;
			OnPropertyChanged(nameof(Sum));

			performance.CheckPoint("Sum");
			globalProgress.Add();
			Owners = UoW.GetAll<Owner>().ToList();
			performance.CheckPoint("Owners");
			globalProgress.Close();
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
			foreach(var employee in employees) {
				employee.FillWearInStockInfo(UoW, BaseParameters, Entity.Warehouse, Entity.Date);
			}
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
						stockRepository.StockBalances(UoW,Entity.Warehouse,item.ProtectionTools.Nomenclatures,Entity.Date)
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

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(nameof(Entity.Warehouse) == e.PropertyName) 
				OnPropertyChanged(nameof(SensitiveAddButton));
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
