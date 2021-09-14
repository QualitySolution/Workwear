using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Dialog.ViewModels;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Company;
using workwear.ViewModels.Regulations;
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
			CalculateTotal();
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

		private CollectiveExpenseItem selectedItem;
		public virtual CollectiveExpenseItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
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
			var selectJournal = navigation.OpenViewModel<EmployeeJournalViewModel>(сollectiveExpenseViewModel, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddEmployees;
		}

		public void AddEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var employeeIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id).ToArray();
			var progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(сollectiveExpenseViewModel);
			var progress = progressPage.ViewModel.Progress;

			progress.Start(employeeIds.Length * 2 + 1, text: "Загружаем сотрудников");
			var employees = UoW.Query<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.List();
			foreach(var employee in employees) {
				progress.Add(text: employee.ShortName);
				employee.FillWearInStockInfo(UoW, BaseParameters, Warehouse, Entity.Date);
				progress.Add();
				Entity.AddItems(employee, BaseParameters);
			}
			CalculateTotal();
			progress.Close();
			navigation.ForceClosePage(progressPage, CloseSource.FromParentPage);
		}

		public void ShowAllSize(CollectiveExpenseItem item)
		{
			var selectJournal = navigation.OpenViewModel<StockBalanceJournalViewModel>(сollectiveExpenseViewModel, QS.Navigation.OpenPageOptions.AsSlave);

			selectJournal.ViewModel.Filter.Warehouse = сollectiveExpenseViewModel.Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.Filter.ProtectionTools = item.ProtectionTools;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += AddNomenclatureProtectionTools;
		}

		public void AddNomenclatureProtectionTools(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var page = navigation.FindPage((DialogViewModelBase)sender);
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var item = page.Tag as CollectiveExpenseItem;
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
