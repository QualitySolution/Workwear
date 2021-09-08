using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using Workwear.Measurements;

namespace workwear.ViewModels.Stock
{
	public class CollectiveExpenseItemsViewModel : ViewModelBase
	{
		public readonly CollectiveExpenseViewModel сollectiveExpenseViewModel;
		private readonly FeaturesService featuresService;
		private readonly INavigationManager navigation;
		private readonly IDeleteEntityService deleteService;

		public SizeService SizeService { get; }
		public BaseParameters BaseParameters { get; }

		public CollectiveExpenseItemsViewModel(CollectiveExpenseViewModel сollectiveExpenseViewModel, FeaturesService featuresService, INavigationManager navigation, SizeService sizeService, IDeleteEntityService deleteService, BaseParameters baseParameters)
		{
			this.сollectiveExpenseViewModel = сollectiveExpenseViewModel ?? throw new ArgumentNullException(nameof(сollectiveExpenseViewModel));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.deleteService = deleteService ?? throw new ArgumentNullException(nameof(deleteService));
			BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));

			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.ObservableItems.ListContentChanged += ExpenceDoc_ObservableItems_ListContentChanged;
		}

		#region Хелперы
		private IUnitOfWork UoW => сollectiveExpenseViewModel.UoW;
		private CollectiveExpense Entity => сollectiveExpenseViewModel.Entity;
		#endregion

		#region Поля

		public GenericObservableList<CollectiveExpenseItem> ObservableItems => Entity.ObservableItems;

		private string sum;
		public virtual string Sum {
			get => sum;
			set => SetField(ref sum, value);
		}

		public virtual Warehouse Warehouse {
			get { return Entity.Warehouse; }
			set { Entity.Warehouse = value; }
		}

		#endregion
		#region Sensetive
		public bool SensetiveAddButton => Entity.Warehouse != null;
		#endregion
		#region Visible
		public bool VisibleSignColumn => featuresService.Available(WorkwearFeature.IdentityCards);
		#endregion
		#region Действия View
		public void AddItem()
		{
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(сollectiveExpenseViewModel, QS.Navigation.OpenPageOptions.AsSlave);

			selectJournal.ViewModel.Filter.Warehouse = сollectiveExpenseViewModel.Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature;
		}

		public void ShowAllSize(CollectiveExpenseItem item)
		{
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(сollectiveExpenseViewModel, QS.Navigation.OpenPageOptions.AsSlave);

			selectJournal.ViewModel.Filter.Warehouse = сollectiveExpenseViewModel.Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.Filter.ProtectionTools = item.ProtectionTools;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += AddNomenclatureProtectionTools;
		}

		public void AddNomenclature(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				сollectiveExpenseViewModel.Entity.AddItem(node.GetStockPosition(сollectiveExpenseViewModel.UoW));
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

		public void Delete(CollectiveExpenseItem item)
		{
			if(item.Id > 0)
				deleteService.DeleteEntity<CollectiveExpenseItem>(item.Id, UoW, () => Entity.RemoveItem(item));
			else
				Entity.RemoveItem(item);
			CalculateTotal();
		}

		public void OpenNomenclature(Nomenclature nomenclature)
		{
			navigation.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(сollectiveExpenseViewModel, EntityUoWBuilder.ForOpen(nomenclature.Id));
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

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(nameof(Entity.Warehouse) == e.PropertyName)
				OnPropertyChanged(nameof(SensetiveAddButton));
		}
	}
}
