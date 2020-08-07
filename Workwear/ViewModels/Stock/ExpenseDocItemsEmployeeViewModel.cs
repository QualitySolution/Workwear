using System;
using System.Linq;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;

namespace workwear.ViewModels.Stock
{
	public class ExpenseDocItemsEmployeeViewModel : ViewModelBase
	{
		public readonly ExpenseEmployeeViewModel expenseEmployeeViewModel;
		private readonly INavigationManager navigation;

		public ExpenseDocItemsEmployeeViewModel(ExpenseEmployeeViewModel expenseEmployeeViewModel, INavigationManager navigation)
		{
			this.expenseEmployeeViewModel = expenseEmployeeViewModel ?? throw new ArgumentNullException(nameof(expenseEmployeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

			Entity.ObservableItems.ListContentChanged += ExpenceDoc_ObservableItems_ListContentChanged;
			Entity.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
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

		public Subdivision Subdivision {
			get { return Entity.Subdivision; }
			set { Entity.Subdivision = value; }
		}

		public ExpenseOperations Operation {
			get { return Entity.Operation; }
			set { Entity.Operation = value; }
		}

		public virtual Warehouse Warehouse {
			get { return Entity.Warehouse; }
			set { Entity.Warehouse = value; }
		}

		#endregion
		#region Sensetive

		public bool SensetiveFillBuhDoc => Entity.Items.Count > 0;

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
			Entity.RemoveItem(item);
			CalculateTotal();
		}

		public void OpenNomenclature(Nomenclature nomenclature)
		{
			navigation.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(expenseEmployeeViewModel, EntityUoWBuilder.ForOpen(nomenclature.Id));
		}
		#endregion

		public void CalculateTotal()
		{
			Sum = String.Format("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>",
				Entity.Items.Count,
				Entity.Items.Sum(x => x.Amount)
			);
		}

		void ExpenceDoc_ObservableItems_ListContentChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{ 
			if(e.PropertyName == nameof(ExpenseItem.BuhDocument)) {
				expenseEmployeeViewModel.HasChanges = true;
			}

		}

	}
}
