using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Report.ViewModels;
using QS.Report;
using QS.ViewModels.Dialog;
using QS.ViewModels;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Stock;
using Workwear.Repository.Operations;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.Tools;
using Workwear.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using workwear;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;

namespace Workwear.ViewModels.Stock
{
	public class ExpenseDocItemsEmployeeViewModel : ViewModelBase
	{
		public readonly ExpenseEmployeeViewModel expenseEmployeeViewModel;
		public readonly FeaturesService featuresService;
		private readonly INavigationManager navigation;
		private readonly IInteractiveQuestion interactive;
		private readonly IDeleteEntityService deleteService;
		private readonly EmployeeIssueRepository employeeRepository;
		private readonly BarcodeService barcodeService;

		public SizeService SizeService { get; }
		public BaseParameters BaseParameters { get; }
		public IList<Owner> Owners { get; }

		public ExpenseDocItemsEmployeeViewModel(
			ExpenseEmployeeViewModel expenseEmployeeViewModel, 
			FeaturesService featuresService, 
			INavigationManager navigation,
			IInteractiveQuestion interactive,
			SizeService sizeService, 
			IDeleteEntityService deleteService,
			BaseParameters baseParameters,
			BarcodeService barcodeService,
			IList<Owner> owners)
		{
			this.expenseEmployeeViewModel = expenseEmployeeViewModel ?? throw new ArgumentNullException(nameof(expenseEmployeeViewModel));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			Owners = owners;
			
			Entity.Items.ContentChanged += ExpenseDoc_ObservableItems_ListContentChanged;
			Entity.PropertyChanged += ExpenseDoc_PropertyChanged;
		}

		#region Хелперы
		private IUnitOfWork UoW => expenseEmployeeViewModel.UoW;
		private Expense Entity => expenseEmployeeViewModel.Entity;
		#endregion

		#region Поля
		public IObservableList<ExpenseItem> ObservableItems => Entity.Items;
		
		public List<ProtectionTools> EmployeesProtectionToolsList  => Entity.Employee.WorkwearItems.Select(x => x.ProtectionTools).ToList();

		private string sum;
		public virtual string Sum {
			get => sum;
			set => SetField(ref sum, value);
		}

		private ExpenseItem selectedItem;
		

		public virtual ExpenseItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}

		#endregion
		#region Sensetive
		public bool SensitiveCreateBarcodes => Entity.Items.Any(x => (x.Nomenclature?.UseBarcode ?? false)
			&& (x.EmployeeIssueOperation?.BarcodeOperations.Count ?? 0) != x.Amount);
		public bool SensitiveBarcodesPrint => Entity.Items.Any(x => x.Amount > 0 
			&& ((x.Nomenclature?.UseBarcode ?? false) || (x.EmployeeIssueOperation?.BarcodeOperations.Count ?? 0) > 0));
		#endregion
		#region Visible
		public bool VisibleSignColumn => featuresService.Available(WorkwearFeature.IdentityCards);
		public bool VisibleBarcodes => featuresService.Available(WorkwearFeature.Barcodes);
		public bool CanAddItems => Entity.Warehouse != null && Entity.Employee != null;
		#endregion
		#region Действия View
		public void AddItem()
		{
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(expenseEmployeeViewModel, QS.Navigation.OpenPageOptions.AsSlave);

			selectJournal.ViewModel.Filter.Warehouse = expenseEmployeeViewModel.Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature;
		}

		public void ShowAllSize(ExpenseItem item)
		{
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(expenseEmployeeViewModel, QS.Navigation.OpenPageOptions.AsSlave);

			selectJournal.ViewModel.Filter.Warehouse = expenseEmployeeViewModel.Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.Filter.ProtectionTools = item.ProtectionTools;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += AddNomenclatureProtectionTools;
		}

		public void AddNomenclature(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var stockPosition = node.GetStockPosition(expenseEmployeeViewModel.UoW);
				var normItem = Entity.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.Nomenclatures
							.Contains(stockPosition.Nomenclature))?.ActiveNormItem;
				var item = expenseEmployeeViewModel.Entity.AddItem(stockPosition);
				if(normItem != null
				   && interactive.Question($"Считать \"{stockPosition.Nomenclature.Name}\"," +
				                           $" как выданное по норме \"{normItem.ProtectionTools.Name}\"?")) {
					item.EmployeeCardItem = Entity.Employee.WorkwearItems
						.FirstOrDefault(x => x.ProtectionTools == normItem.ProtectionTools);
					item.ProtectionTools = normItem.ProtectionTools;
					item.UpdateOperations(UoW,BaseParameters,interactive);
				}
			}
			CalculateTotal();
		}

		public void OpenJournalChangeProtectionTools(ExpenseItem item) {
			var selectJournal = navigation.OpenViewModel<ProtectionToolsJournalViewModel>(expenseEmployeeViewModel, QS.Navigation.OpenPageOptions.AsSlave);
			
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += ChangeProtectionToolsFromJournal;
		}
		private void ChangeProtectionToolsFromJournal(object sender, JournalSelectedEventArgs e) {
			var page = navigation.FindPage((DialogViewModelBase)sender);
			var item = page.Tag as ExpenseItem;
			ChangeProtectionTools(item, UoW.GetById<ProtectionTools>(e.SelectedObjects.First().GetId()));
		}

		public void ChangeProtectionTools(ExpenseItem item, ProtectionTools protectionTools) {
			List<ProtectionTools> protectionToolsForUpdate = new List<ProtectionTools> { protectionTools };
			if (item.ProtectionTools != null) 
				protectionToolsForUpdate.Add(item.ProtectionTools);
			
			item.ProtectionTools = protectionTools;
			item.EmployeeCardItem = Entity.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools);
		}
		
		public void MakeEmptyProtectionTools(ExpenseItem item) {
			item.ProtectionTools = null;
		}
		
		public void AddNomenclatureProtectionTools(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var page = navigation.FindPage((DialogViewModelBase)sender);
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var item = page.Tag as ExpenseItem;
				item.StockPosition = node.GetStockPosition(UoW);
			}
			CalculateTotal();
		}

		public void Delete(ExpenseItem item)
		{
			if(item.Id > 0) {
				if(UoW.HasChanges)
					if(!expenseEmployeeViewModel.Save())//Сохраняем документ если есть добавленные строки или другие изменения. Иначе получим исключение.
						return;
				deleteService.DeleteEntity<ExpenseItem>(item.Id, UoW, () => Entity.RemoveItem(item));
			}
			else
				Entity.RemoveItem(item);
			CalculateTotal();
		}

		public void OpenNomenclature(ExpenseItem item)
		{
			navigation.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(expenseEmployeeViewModel, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}

		public void OpenProtectionTools(ExpenseItem item)
		{
			navigation.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(expenseEmployeeViewModel, EntityUoWBuilder.ForOpen(item.ProtectionTools.Id));
		}

		#region Штрих коды
		public string ButtonCreateOrRemoveBarcodesTitle => 
			Entity.Items.Any(x => (x.Nomenclature?.UseBarcode ?? false) && (x.EmployeeIssueOperation?.BarcodeOperations.Count ?? 0) > x.Amount)
			? "Обновить штрихкоды" : "Создать штрихкоды";
		
		public void ReleaseBarcodes() {
			expenseEmployeeViewModel.SkipBarcodeCheck = true;
			var saveResult = expenseEmployeeViewModel.Save();
			expenseEmployeeViewModel.SkipBarcodeCheck = false;
			if(!saveResult)
				return;

			var operations = Entity.Items.Where(i => i.Nomenclature?.UseBarcode ?? false).Select(x => x.EmployeeIssueOperation).ToList();
			barcodeService.CreateOrRemove(UoW, operations);
			UoW.Commit();
			OnPropertyChanged(nameof(SensitiveCreateBarcodes));
			OnPropertyChanged(nameof(ButtonCreateOrRemoveBarcodesTitle));
		}

		public void PrintBarcodes() {
			if(SensitiveCreateBarcodes) {
				if(interactive.Question("Не для всех строк документа были созданы штрих коды. Обновить штрихкоды?"))
					ReleaseBarcodes();
				else
					return;
			}

			var reportInfo = new ReportInfo {
				Title = "Штрихкоды",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> {
					{
						"barcodes", Entity.Items
							.SelectMany(x => x.EmployeeIssueOperation?.BarcodeOperations.Select(b => b.Barcode?.Id))
							.Where(x => x != null)
							.ToList()
					}
				}
			};

			navigation.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
		#endregion
		#endregion

		#region Расчет для View
		public string GetRowColor(ExpenseItem item) {
			if(item.EmployeeCardItem?.Graph == null)
				return null;
			var requiredIssue = item.EmployeeCardItem?.CalculateRequiredIssue(BaseParameters, Entity.Date);
			if(item.ProtectionTools?.Type.IssueType == IssueType.Collective) 
				return item.Amount > 0 ? "#7B3F00" : "Burlywood";
			if(requiredIssue > 0 && item.Nomenclature == null)
				return item.Amount == 0 ? "red" : "Dark red";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			if(requiredIssue > item.Amount)
				return "blue";
			if(requiredIssue < item.Amount)
				return "Purple";
			if(item.EmployeeCardItem?.NextIssue > DateTime.Today)
				return "darkgreen";
			return null;
		}
		#endregion

		public void CalculateTotal()
		{
			Sum = String.Format("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>",
				Entity.Items.Count,
				Entity.Items.Sum(x => x.Amount)
			);
		}

		#region События
		
		void ExpenseDoc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Warehouse) || e.PropertyName == nameof(Entity.Employee))
				OnPropertyChanged(nameof(CanAddItems));
		}
		private void ExpenseDoc_ObservableItems_ListContentChanged(object sender, EventArgs e)
		{
			CalculateTotal();
			OnPropertyChanged(nameof(SensitiveCreateBarcodes));
			OnPropertyChanged(nameof(SensitiveBarcodesPrint));
			OnPropertyChanged(nameof(ButtonCreateOrRemoveBarcodesTitle));
		}
		#endregion
	}
}
