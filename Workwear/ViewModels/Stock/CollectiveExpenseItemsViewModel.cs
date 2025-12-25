using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Utilities.Debug;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.Filter.ViewModels.Stock;
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
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public readonly CollectiveExpenseViewModel collectiveExpenseViewModel;
		public readonly FeaturesService featuresService;
		private readonly INavigationManager navigation;
		private readonly IInteractiveMessage interactive;
		private readonly ICurrentPermissionService permissionService;
		private readonly ModalProgressCreator modalProgress;
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
			ICurrentPermissionService permissionService,
			BaseParameters baseParameters,
			ModalProgressCreator modalProgress,
			PerformanceHelper performance //Только для использования в конструкторе. Шаги запуска.
			)
		{
			this.collectiveExpenseViewModel = collectiveExpenseViewModel ?? throw new ArgumentNullException(nameof(collectiveExpenseViewModel));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
			this.modalProgress = modalProgress ?? throw new ArgumentNullException(nameof(modalProgress));
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
			Entity.Items.ContentChanged += ExpenseDoc_ObservableItems_ListContentChanged;
			Owners = allOwners.ToList();
		}

		#region Хелперы
		private IUnitOfWork UoW => collectiveExpenseViewModel.UoW;
		public CollectiveExpense Entity => collectiveExpenseViewModel.Entity;
		#endregion
		#region Поля
		public string Sum => $"Строк в документе: <u>{Entity.Items.Count}</u>" +
				$" Сотрудников: <u>{Entity.Items.Select(x => x.Employee.Id).Distinct().Count()}</u>" +
				$" Единиц продукции: <u>{Entity.Items.Sum(x => x.Amount)}</u>";

		private CollectiveExpenseItem selectedItem;
		[PropertyChangedAlso(nameof(SensitiveButtonDel))]
		[PropertyChangedAlso(nameof(SensitiveButtonChange))]
		[PropertyChangedAlso(nameof(SensitiveRefreshMenuItem))]
		[PropertyChangedAlso(nameof(CountItemsForEmployee))]
		[PropertyChangedAlso(nameof(CountItemsForProtectionTools))]
		public virtual CollectiveExpenseItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}
		
		public int CountItemsForEmployee => SelectedItem?.Employee != null ? Entity.Items.Count(x => x.Employee.IsSame(SelectedItem?.Employee)) : 0;
		public int CountItemsForProtectionTools => SelectedItem?.ProtectionTools != null ? Entity.Items.Count(x => x.ProtectionTools.IsSame(SelectedItem?.ProtectionTools)) : 0;
		public IList<Owner> Owners { get; }

		#endregion
		#region Sensetive
		public bool CanEdit => permissionService.ValidateEntityPermission(typeof(CollectiveExpense), Entity.Date).CanUpdate;
		public bool SensitiveAddButton => CanEdit && Entity.Warehouse != null;
		public bool SensitiveButtonChange => CanEdit && SelectedItem != null && Entity.Warehouse != null;
		public bool SensitiveButtonDel => CanEdit && SelectedItem != null;
		public bool SensitiveRefreshMenuItem => SelectedItem != null;
		public bool SensitiveRefreshAllMenuItem => Entity.Items.Any();
		#endregion
		#region Visible
		public bool VisibleSignColumn => featuresService.Available(WorkwearFeature.IdentityCards);
		public bool VisibleEmployeeGroup => featuresService.Available(WorkwearFeature.EmployeeGroups);
		#endregion
		#region Действия View

		public void AddEmployees(){
			var selectJournal = navigation.OpenViewModel<EmployeeJournalViewModel>(collectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.Filter.ShowOnlyWork = true;
			selectJournal.ViewModel.OnSelectResult += LoadEmployees;
		}
		private void LoadEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {

			var performance = new ProgressPerformanceHelper(modalProgress, 6, "Загружаем...", logger, showProgressText: true);
			var employeeIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id).ToArray();

			AddEmployeesList(employeeIds, performance);
		}

		public void AddSubdivisions() {
			var selectJournal = navigation.OpenViewModel<SubdivisionJournalViewModel>(collectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadSubdivisions;
		}
		private void LoadSubdivisions(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {

			var performance = new ProgressPerformanceHelper(modalProgress, 6, "Ищем сотрудников", logger, showProgressText: true);
			var subdivisionIds = e.GetSelectedObjects<SubdivisionJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromSubdivisions(UoW, subdivisionIds);

			AddEmployeesList(employees, performance);
		}

		public void AddDepartments() {
			var selectJournal = navigation.OpenViewModel<DepartmentJournalViewModel>(collectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadDepartments;
		}
		private void LoadDepartments(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var performance = new ProgressPerformanceHelper(modalProgress, 6, "Ищем сотрудников", logger, showProgressText: true);
			var departmentsIds = e.GetSelectedObjects<DepartmentJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromDepartments(UoW, departmentsIds);
			
			AddEmployeesList(employees, performance);
		}
		
		public void AddEmployeeGroups() {
			var selectJournal = navigation.OpenViewModel<EmployeeGroupJournalViewModel>(collectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadEmployeeGroups;
		}
		private void LoadEmployeeGroups(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var performance = new ProgressPerformanceHelper(modalProgress, 6, "Ищем сотрудников", logger, showProgressText: true);
			var groupsIds = e.GetSelectedObjects<EmployeeGroupJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromGroups(UoW, groupsIds);
			
			AddEmployeesList(employees, performance);
		}

		public void AddEmployeesList(IEnumerable<EmployeeCard> employees, ProgressPerformanceHelper performance) {
			AddEmployeesList(employees.Select(x => x.Id).ToArray(), performance);
		}

		private void AddEmployeesList(int[] employeeIds, ProgressPerformanceHelper performance) {
			if(!employeeIds.Any()) {
				performance.End();
				interactive.ShowMessage(ImportanceLevel.Info, "Нет сотрудников для добавления");
				return;
			}
			
			performance.CheckPoint("Загружаем информацию о сотрудниках");
			var employees = issueModel.PreloadEmployeeInfo(employeeIds);
			
			performance.CheckPoint("Загружаем потребности");
			issueModel.PreloadWearItems(employeeIds);
			
			performance.CheckPoint("Загружаем прошлые выдачи");
			issueModel.FillWearReceivedInfo(employees.ToArray());
			
			performance.CheckPoint("Загружаем складские остатки");
			issueModel.FillWearInStockInfo(employees, stockBalanceModel);
			
			performance.CheckPoint("Подготавливаем список потребностей");
			Dictionary<int, IssueWidgetItem> widgetList = new Dictionary<int, IssueWidgetItem>();
			
			var needs = employees
				.SelectMany(x => x.WorkwearItems)
				.Where(x=> !Entity.Items.Any(y =>y.EmployeeCardItem == x))
				.ToList();
			
			foreach(var item in needs) {
				if(widgetList.ContainsKey(item.ProtectionTools.Id)) {
					widgetList[item.ProtectionTools.Id].NumberOfNeeds++;
					widgetList[item.ProtectionTools.Id].ItemQuantityForIssuse += item.CalculateRequiredIssue(BaseParameters, Entity.Date);
					if(item.CalculateRequiredIssue(BaseParameters, Entity.Date) != 0)
						widgetList[item.ProtectionTools.Id].NumberOfCurrentNeeds++;
				}
				else
					widgetList.Add(item.ProtectionTools.Id, new IssueWidgetItem(item.ProtectionTools,
						item.ProtectionTools.Type.IssueType == IssueType.Collective,
						item.CalculateRequiredIssue(BaseParameters, Entity.Date)>0 ? 1 : 0,
						1,
						item.CalculateRequiredIssue(BaseParameters, Entity.Date),
						stockBalanceModel.ForNomenclature(item.ProtectionTools.Nomenclatures.ToArray())
								.Sum(x =>x.Amount)));
			}

			performance.End();
			if(!widgetList.Any()) {
				interactive.ShowMessage(ImportanceLevel.Info, "Нет потребностей для добавления");
				return;
			}

			var page = navigation.OpenViewModel<IssueWidgetViewModel, Dictionary<int, IssueWidgetItem>>
				(null, widgetList.OrderByDescending(x => x.Value.Active).ThenBy(x=>x.Value.ProtectionTools.Name)
					.ToDictionary(x => x.Key, x => x.Value));
			
			page.ViewModel.AddItems = (dic,vac) => AddItemsFromWidget(dic, needs, page, vac);
			logger.Info($"Диалог выбора потребностей открыт за {performance.TotalTime.TotalSeconds} секунд.");
		}

		public void AddItemsFromWidget(Dictionary<int, IssueWidgetItem> widgetItems, List<EmployeeCardItem> needs,
			IPage page, bool excludeOnVacation) {
			foreach(var item in needs) {
				if(widgetItems.First(x => x.Key == item.ProtectionTools.Id).Value.Active)
					if(excludeOnVacation && item.EmployeeCard.OnVacation(Entity.Date))
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
			var selectJournal = navigation.OpenViewModel<StockBalanceJournalViewModel>(collectiveExpenseViewModel, QS.Navigation.OpenPageOptions.AsSlave,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
						filter => {
							filter.WarehouseEntry.IsEditable = false;
							filter.Warehouse = collectiveExpenseViewModel.Entity.Warehouse;
							filter.ProtectionTools = items.First().ProtectionTools;
						});
				});
			
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
			UoW.Delete(deleteItem.WarehouseOperation);
			OnPropertyChanged(nameof(Sum));
		}

		public void DeleteEmployee(CollectiveExpenseItem item)
		{
			var toDelete = Entity.Items.Where(x => x.Employee.IsSame(item.Employee)).ToList();
			foreach(var deleteItem in  toDelete) {
				DeleteItem(deleteItem);
			}
		}
		
		public void DeleteProtectionTools(CollectiveExpenseItem item) {
			var toDelete = Entity.Items.Where(x => x.ProtectionTools.IsSame(item.ProtectionTools)).ToList();
			foreach(var deleteItem in toDelete) {
				DeleteItem(deleteItem);
			}
		}

		#endregion
		#region Расчет для View
		public string GetRowColor(CollectiveExpenseItem item) {
			if(item.Id != 0 && item.Amount <= 0)
				return "red";
			if(item.Id != 0)
				return null;
			var requiredIssue = item.EmployeeCardItem?.CalculateRequiredIssue(BaseParameters, Entity.Date);
			if(requiredIssue > 0 && item.Nomenclature == null)
				return item.Amount == 0 ? "red" : "Dark red";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			if(requiredIssue > item.Amount)
				return "blue";
			return null;
		}

		#endregion
		#region Обновление документа

		public void Refresh(CollectiveExpenseItem[] selectedCollectiveExpenseItem) {
			var performance = new ProgressPerformanceHelper(modalProgress, 6, "Загружаем...", logger, showProgressText: true);
			AddEmployeesList(selectedCollectiveExpenseItem?.Select(x => x.Employee).Distinct(), performance);
		}
		public void RefreshAll() {
			var performance = new ProgressPerformanceHelper(modalProgress, 6, "Загружаем...", logger, showProgressText: true);
			AddEmployeesList(Entity.Items.Select(x => x.Employee).Distinct(), performance);
		}

		#endregion
		#region Открытие
		public void OpenEmployee(CollectiveExpenseItem item)
		{
			navigation.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(collectiveExpenseViewModel, EntityUoWBuilder.ForOpen(item.Employee.Id));
		}

		public void OpenNomenclature(CollectiveExpenseItem item)
		{
			navigation.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(collectiveExpenseViewModel, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}

		public void OpenProtectionTools(CollectiveExpenseItem item)
		{
			navigation.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(collectiveExpenseViewModel, EntityUoWBuilder.ForOpen(item.ProtectionTools.Id));
		}
		#endregion
		#region Обработка событий
		private void ExpenseDoc_ObservableItems_ListContentChanged(object sender, EventArgs e) {
			OnPropertyChanged(nameof(SensitiveRefreshAllMenuItem));
			OnPropertyChanged(nameof(Sum));
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
				case nameof(Entity.Warehouse):
					stockBalanceModel.Warehouse = Entity.Warehouse;
					OnPropertyChanged(nameof(SensitiveAddButton));
					OnPropertyChanged(nameof(SensitiveButtonChange));
					break;
				case nameof(Entity.Date):
					stockBalanceModel.OnDate = Entity.Date;
					break;
			}
		}
		#endregion
	}
}
