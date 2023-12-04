using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.ViewModels;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Stock
{
	public class ExpenseDocItemsObjectViewModel : ViewModelBase
	{
		public readonly ExpenseObjectViewModel expenseObjectViewModel;
		private readonly INavigationManager navigation;
		private readonly IDeleteEntityService deleteService;
		public readonly FeaturesService featuresService;
		public IList<Owner> Owners { get; }

		public ExpenseDocItemsObjectViewModel(
			ExpenseObjectViewModel expenseObjectViewModel, 
			INavigationManager navigation, 
			IDeleteEntityService deleteService,
			FeaturesService featuresService)
		{
			this.expenseObjectViewModel = expenseObjectViewModel ?? throw new ArgumentNullException(nameof(expenseObjectViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));

			Entity.Items.ContentChanged += ExpenseDoc_ObservableItems_ListContentChanged;
			Owners = UoW.GetAll<Owner>().ToList();
		}

		#region Хелперы
		private IUnitOfWork UoW => expenseObjectViewModel.UoW;
		private Expense Entity => expenseObjectViewModel.Entity;
		#endregion

		#region Получение данных
		public IList<SubdivisionPlace> Places => Entity.Subdivision?.Places;
		#endregion

		#region Поля
		public IObservableList<ExpenseItem> ObservableItems => Entity.Items;

		private string sum;
		public virtual string Sum {
			get => sum;
			set => SetField(ref sum, value);
		}

		#endregion

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

		private void ExpenseDoc_ObservableItems_ListContentChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}
	}
}
