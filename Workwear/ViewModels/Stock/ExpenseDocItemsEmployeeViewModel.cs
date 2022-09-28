using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Regulations;
using Workwear.Measurements;
using Workwear.Repository.Operations;

namespace Workwear.ViewModels.Stock
{
	public class ExpenseDocItemsEmployeeViewModel : ViewModelBase
	{
		public readonly ExpenseEmployeeViewModel expenseEmployeeViewModel;
		public readonly FeaturesService featuresService;
		private readonly INavigationManager navigation;
		private readonly IDeleteEntityService deleteService;
		private readonly EmployeeIssueRepository employeeRepository;

		public SizeService SizeService { get; }
		public BaseParameters BaseParameters { get; }
		public IList<Owner> Owners { get; }

		public ExpenseDocItemsEmployeeViewModel(
			ExpenseEmployeeViewModel expenseEmployeeViewModel, 
			FeaturesService featuresService, 
			INavigationManager navigation, 
			SizeService sizeService, 
			IDeleteEntityService deleteService,
			EmployeeIssueRepository employeeRepository,
			BaseParameters baseParameters)
		{
			this.expenseEmployeeViewModel = expenseEmployeeViewModel ?? throw new ArgumentNullException(nameof(expenseEmployeeViewModel));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			employeeRepository.RepoUow = UoW;
			BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			Entity.ObservableItems.ListContentChanged += ExpenceDoc_ObservableItems_ListContentChanged;
			Entity.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
			Entity.PropertyChanged += EntityOnPropertyChanged;
			Entity.FillCanWriteoffInfo(employeeRepository);
			Owners = UoW.GetAll<Owner>().ToList();
		}

		#region Хелперы
		private IUnitOfWork UoW => expenseEmployeeViewModel.UoW;
		private Expense Entity => expenseEmployeeViewModel.Entity;
		#endregion

		#region Поля

		public System.Data.Bindings.Collections.Generic.GenericObservableList<ExpenseItem> ObservableItems {
			get { return Entity.ObservableItems; }
		}

		private string sum;
		public virtual string Sum {
			get => sum;
			set => SetField(ref sum, value);
		}

		public virtual Warehouse Warehouse {
			get { return Entity.Warehouse; }
			set { Entity.Warehouse = value; }
		}

		private ExpenseItem selectedItem;
		public virtual ExpenseItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}

		#endregion
		#region Sensetive
		public bool SensetiveFillBuhDoc => Entity.Items.Count > 0;
		#endregion
		#region Visible
		public bool VisibleSignColumn => featuresService.Available(WorkwearFeature.IdentityCards);
		#endregion
		#region Действия View
		public void FillBuhDoc()
		{
			using(var dlg = new Dialog("Введите бухгалтерский документ", MainClass.MainWin, DialogFlags.Modal)) {
				var docEntry = new Entry(80);
				if(expenseEmployeeViewModel.Entity.Items.Count > 0)
					docEntry.Text = expenseEmployeeViewModel.Entity.Items.First().BuhDocument;
				docEntry.TooltipText = "Бухгалтерский документ по которому была произведена выдача. Отобразится вместо подписи сотрудника в карточке.";
				docEntry.ActivatesDefault = true;
				dlg.VBox.Add(docEntry);
				dlg.AddButton("Заменить", ResponseType.Ok);
				dlg.AddButton("Отмена", ResponseType.Cancel);
				dlg.DefaultResponse = ResponseType.Ok;
				dlg.ShowAll();
				if(dlg.Run() == (int)ResponseType.Ok) {
					expenseEmployeeViewModel.Entity.ObservableItems.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
				}
				dlg.Destroy();
			}
		}

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
				expenseEmployeeViewModel.Entity.AddItem(node.GetStockPosition(expenseEmployeeViewModel.UoW));
			}
			CalculateTotal();
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
				if(Entity.Items.Any(x => x.Id == 0))
					expenseEmployeeViewModel.Save(); //Сохраняем документ если есть добавленные строки. Иначе получим исключение.
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
		#endregion

		public void CalculateTotal()
		{
			Sum = String.Format("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>",
				Entity.Items.Count,
				Entity.Items.Sum(x => x.Amount)
			);
		}

		#region События
		private void ExpenceDoc_ObservableItems_ListContentChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{ 
			if(e.PropertyName == nameof(ExpenseItem.BuhDocument)) {
				expenseEmployeeViewModel.HasChanges = true;
			}
		}

		private void EntityOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Date))
				Entity.FillCanWriteoffInfo(employeeRepository);
		}
		#endregion
	}
}
