using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.ViewModels;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;

namespace workwear.ViewModels.Stock
{
	public class ExpenseDocItemsObjectViewModel : ViewModelBase
	{
		public readonly ExpenseObjectViewModel expenseObjectViewModel;
		private readonly INavigationManager navigation;
		private readonly IDeleteEntityService deleteService;

		public ExpenseDocItemsObjectViewModel(ExpenseObjectViewModel expenseObjectViewModel, INavigationManager navigation, IDeleteEntityService deleteService)
		{
			this.expenseObjectViewModel = expenseObjectViewModel ?? throw new ArgumentNullException(nameof(expenseObjectViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));

			Entity.ObservableItems.ListContentChanged += ExpenceDoc_ObservableItems_ListContentChanged;
			Entity.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
		}

		#region Хелперы
		private IUnitOfWork UoW => expenseObjectViewModel.UoW;
		private Expense Entity => expenseObjectViewModel.Entity;
		#endregion

		#region Получение данных
		public IList<SubdivisionPlace> Places => Entity.Subdivision?.Places;
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

		#endregion
		#region Sensetive

		public bool SensetiveFillBuhDoc => Entity.Items.Count > 0;

		#endregion

		public void FillBuhDoc()
		{
			using(var dlg = new Dialog("Введите бухгалтерский документ", MainClass.MainWin, DialogFlags.Modal)) {
				var docEntry = new Entry(80);
				if(expenseObjectViewModel.Entity.Items.Count > 0)
					docEntry.Text = expenseObjectViewModel.Entity.Items.First().BuhDocument;
				docEntry.TooltipText = "Бухгалтерский документ по которому была произведена выдача. Отобразится вместо подписи сотрудника в карточке.";
				docEntry.ActivatesDefault = true;
				dlg.VBox.Add(docEntry);
				dlg.AddButton("Заменить", ResponseType.Ok);
				dlg.AddButton("Отмена", ResponseType.Cancel);
				dlg.DefaultResponse = ResponseType.Ok;
				dlg.ShowAll();
				if(dlg.Run() == (int)ResponseType.Ok) {
					expenseObjectViewModel.Entity.ObservableItems.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
				}
				dlg.Destroy();
			}
		}

		public void AddItem()
		{
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(expenseObjectViewModel, QS.Navigation.OpenPageOptions.AsSlave);
			if(expenseObjectViewModel.Entity.Operation == ExpenseOperations.Object)
				selectJournal.ViewModel.Filter.ItemTypeCategory = ItemTypeCategory.property;

			selectJournal.ViewModel.Filter.Warehouse = expenseObjectViewModel.Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature;
		}

		public void AddNomenclature(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				expenseObjectViewModel.Entity.AddItem(node.GetStockPosition(expenseObjectViewModel.UoW));
			}
			CalculateTotal();
		}

		public void Delete(ExpenseItem item)
		{
			if(item.Id > 0) {
				if(Entity.Items.Any(x => x.Id == 0))
					expenseObjectViewModel.Save(); //Сохраняем документ если есть добавленные строки. Иначе получим исключение.
				deleteService.DeleteEntity<ExpenseItem>(item.Id, UoW, () => Entity.RemoveItem(item));
			}
			else
				Entity.RemoveItem(item);
			CalculateTotal();
		}

		public void OpenNomenclature(Nomenclature nomenclature)
		{
			navigation.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(expenseObjectViewModel, EntityUoWBuilder.ForOpen(nomenclature.Id));
		}

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
				expenseObjectViewModel.HasChanges = true;
			}

		}
	}
}
