using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Utilities;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Tools;
using Workwear.Tools.Sizes;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Analytics.WarehouseForecasting;
using Workwear.Models.Operations;
using Workwear.Tools.Features;
using Workwear.Tools.User;
using Workwear.ViewModels.Analytics;
using Workwear.ViewModels.Communications;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Supply {
	public class ShipmentViewModel :EntityDialogViewModelBase<Shipment>, IDialogDocumentation {
		public StockBalanceModel StockBalanceModel { get; set; }
		public ShipmentViewModel(
			BaseParameters baseParameters,
			CurrentUserSettings currentUserSettings,
			FeaturesService featuresService,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveService interactive,
			IUserService userService,
			IEntityChangeWatcher watcher,
			StockBalanceModel stockBalanceModel,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null,
			List<WarehouseForecastingItem> forecastingItems = null,
			ShipmentParams shipmentParameters = null
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			StockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
            			
			if(Entity.Id == 0)
				Entity.CreatedbyUser = userService.GetCurrentUser();

			if(shipmentParameters != null)
				Entity.WarehouseForecastingDate = shipmentParameters.EndDate;
			
			if(forecastingItems != null) 
				AddFromForecasting(forecastingItems, shipmentParameters);

			var warehouses = UoW.GetAll<Warehouse>().ToList();
			if(!featuresService.Available(WorkwearFeature.Warehouses) || warehouses.Count == 1) {
				WarehousesList.Add(new Warehouse(){Id = -1, Name = "Показать"});
			}
			else {
				WarehousesList.Add(new Warehouse(){Id = -1, Name = "На всех складах"});
				WarehousesList.AddRange(warehouses);
			}
			Warehouse = warehousesList.First();
			IsNullWearPercent = false;
			
			CalculateTotal();
			watcher.BatchSubscribe(OnExternalShipmentChange)
				.ExcludeUow(UoW)
				.IfEntity<ShipmentItem>()
				.AndChangeType(TypeOfChangeEvent.Update)
				.AndWhere(x => x.Shipment.Id == Entity.Id);

			Entity.Items.ContentChanged += Shipment_ObservableItems_ListContentChanged;
			Entity.PropertyChanged += EntityOnPropertyChanged;
		}

		private void EntityOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(Entity.Status)) {
				OnPropertyChanged(nameof(CanAddItem));
				OnPropertyChanged(nameof(CanRemoveItem));
				OnPropertyChanged(nameof(CanToOrder));
				OnPropertyChanged(nameof(CanEditDiffCause));
				OnPropertyChanged(nameof(CanEditRequested));
				OnPropertyChanged(nameof(CanEditOrdered));
			}
		}

		private void Shipment_ObservableItems_ListContentChanged(object sender, EventArgs e) {
			FillInStock();
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#planned-shipment");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Свойства ViewModel

		private readonly IInteractiveService interactive;
		private readonly CurrentUserSettings currentUserSettings;
		private readonly BaseParameters baseParameters;
		private readonly SizeService sizeService = new SizeService();
		private readonly FeaturesService featuresService;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}
		
		private ShipmentItem[] selectedItems;
		[PropertyChangedAlso(nameof(CanRemoveItem))]
		[PropertyChangedAlso(nameof(CanToOrder))]
		[PropertyChangedAlso(nameof(CanAddSize))]
		public virtual ShipmentItem[] SelectedItems {
			get=>selectedItems;
			set=>SetField(ref selectedItems, value);
		}

		#endregion

		#region Проброс свойств документа

		public virtual string DocID {
			get {
				return Entity.Id == 0 ? "Новый" : Entity.Id.ToString();
			}
		}
		public virtual UserBase CreatedByUser => Entity.CreatedbyUser;
		public virtual DateTime? StartPeriod { get => Entity.StartPeriod; set => Entity.StartPeriod = value; }
		public virtual DateTime? EndPeriod { get => Entity.EndPeriod; set => Entity.EndPeriod = value; }
		public virtual string DocComment { get => Entity.Comment; set => Entity.Comment = value; }
		public virtual DateTime? WarehouseForecastingDate { get => Entity.WarehouseForecastingDate; set => Entity.WarehouseForecastingDate = value; }
		public virtual IObservableList<ShipmentItem> Items => Entity.Items;
		
		#endregion

		#region Свойства View

		public virtual bool CanAddItem => CanEditRequested;
		public virtual bool CanRemoveItem => CanEditRequested && SelectedItems != null && SelectedItems.Length > 0;
		public virtual bool CanToOrder => SelectedItems != null && SelectedItems.Length > 0 && 
		                                  SelectedItems.Any(i => i.Requested != i.Ordered);
		public virtual bool CanEditDiffCause => Entity.Status != ShipmentStatus.New && Entity.Status != ShipmentStatus.Draft;
		public virtual bool CanEditRequested => Entity.Status == ShipmentStatus.New || Entity.Status == ShipmentStatus.Draft;
		public virtual bool CanEditOrdered => Entity.Status != ShipmentStatus.Ordered || Entity.Status != ShipmentStatus.Received;
		public virtual bool CanSandEmail => featuresService.Available(WorkwearFeature.Communications);
		public virtual bool CanAddSize => SelectedItems != null && SelectedItems.Any(x => x.WearSizeType != null || x.HeightType != null) && SelectedItems.Length == 1;
		public virtual bool VisibleWarehouseForecastingDate => Entity.WarehouseForecastingDate != null;
		
		public virtual IList<Size> GetSizeVariants(ShipmentItem item) {
			return sizeService.GetSize(UoW, item.WearSizeType, onlyUseInNomenclature: true).ToList();
		}
		
		public virtual IList<Size> GetHeightVariants(ShipmentItem item) {
			return sizeService.GetSize(UoW, item.HeightType, onlyUseInNomenclature: true).ToList();
		}

		#endregion

		#region Действия view
		public void AddItem() {
			var selectJournal = NavigationManager
				.OpenViewModel<NomenclatureJournalViewModel>( this,OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.ShowArchival = false;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddItem_OnSelectResult;
		}
		
		private void AddItem_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddItem(n, interactive));
			CalculateTotal();
		}
		
		public void DeleteItems(ShipmentItem[] items) {
			foreach(var item in items)
				Entity.RemoveItem(item);
			OnPropertyChanged(nameof(CanRemoveItem));
			OnPropertyChanged(nameof(CanAddSize));
			CalculateTotal();
		}
		
		public void ToOrderItems() {
			foreach(var item in SelectedItems)
				item.Ordered = item.Requested;
			OnPropertyChanged(nameof(CanToOrder));
		}
		private void CalculateTotal() {
			Total = $"Позиций в документе: {Entity.Items.Count}  " +
			        $"Количество единиц: {Entity.Items.Sum(x => x.Requested)} " +
			        $"Сумма: {Entity.Items.Sum(x => x.Requested * x.Cost)}{baseParameters.UsedCurrency}";
		}

		public void SendMessageForBuyer() {
			var dialog = NavigationManager.OpenViewModel<SendEmailViewModel>(this);
			dialog.ViewModel.EmailAddress = currentUserSettings.Settings.BuyerEmail;
			dialog.ViewModel.Topic = "Новая планируемая поставка Спецодежды.";
			dialog.ViewModel.Message = "Добрый день!\nВ программе QS создана новая заявка на закупку. Просим принять в работу.";
			dialog.ViewModel.Title = "Оповестить закупку";
			
			dialog.ViewModel.ShowSaveAddress = true;
			dialog.ViewModel.SaveAddressFunc = address => {
				currentUserSettings.Settings.BuyerEmail = address;
				currentUserSettings.SaveSettings();
			};
		}
		
		public void AddFromForecasting(List<WarehouseForecastingItem> forecastingItems, ShipmentParams shipmentParameters) {

			var nomIds = forecastingItems
				.Where( i => i.Nomenclature != null)
				.Select(i => i.Nomenclature)
				.Select(n => n.Id).ToList();
			var sizeIds = forecastingItems
				.Where( i => i.Size != null)
				.Select(i => i.Size)
				.Select(n => n.Id).ToList();
			sizeIds.AddRange(forecastingItems
				.Where( i => i.Height != null)
				.Select(i => i.Height)
				.Select(n => n.Id).ToList());
			
			UoW.Session.QueryOver<Nomenclature>()
				.Where(x => x.Id.IsIn(nomIds))
				.Fetch(SelectMode.Fetch, n => n.Type.Units)
				.Future();
			UoW.Session.QueryOver<Size>()
				.Where(x => x.Id.IsIn(sizeIds))
				.Future();
			
			foreach(var fitem in forecastingItems.Where( i => i.Nomenclature != null)) {
				if(shipmentParameters.Type == ShipmentCreateType.WithDebt && fitem.WithDebt < 0 ||
				   shipmentParameters.Type == ShipmentCreateType.WithoutDebt && fitem.WithoutDebt < 0)
					Entity.AddItem(
						UoW.GetInSession(fitem.Nomenclature),
						UoW.GetInSession(fitem.Size),
						UoW.GetInSession(fitem.Height),
						(shipmentParameters.Type == ShipmentCreateType.WithDebt ? fitem.WithDebt : fitem.WithoutDebt) * -1,
						fitem.Nomenclature.SaleCost ?? 0 //Возможно стоит подвязаться на переключатель типа стоимости в прогнозе
						);
			}
		}

		public void AddSize(ShipmentItem[] items) {
			if(items[0].Nomenclature == null)
				return;
			var existItems = Entity.Items
				.Where(x => x.Nomenclature.IsSame(items[0].Nomenclature))
				.Cast<IDocItemSizeInfo>().ToList();
			var selectJournal = NavigationManager
				.OpenViewModel<SizeWidgetViewModel, IDocItemSizeInfo, IUnitOfWork, IList<IDocItemSizeInfo>>
					(null, items[0], UoW, existItems);
			selectJournal.ViewModel.AddedSizes += (s, e) => SelectWearSize_SizeSelected(e, items[0]);
		}

		private void SelectWearSize_SizeSelected(AddedSizesEventArgs e, ShipmentItem item) {
			foreach(var i in e.SizesWithAmount.ToList()) {
				var exist = Entity.FindItem(item.Nomenclature, i.Size, e.Height);
				if(exist != null)
					exist.Requested = i.Amount;
				else
					Entity.AddItem(item.Nomenclature,  i.Size, e.Height, i.Amount, item.Cost);
			}
			if(item.WearSize == null) {
				Entity.RemoveItem(item);
				OnPropertyChanged(nameof(CanRemoveItem));
				OnPropertyChanged(nameof(CanAddSize));
			}
			CalculateTotal();
		}
		#endregion

		private void OnExternalShipmentChange(EntityChangeEvent[] changeEvents) {
			foreach(var change in changeEvents) {
				var myItem = Entity.Items.FirstOrDefault(i => i.Id == change.Entity.GetId());
				if(myItem != null)
					UoW.Session.Refresh(myItem);
			}
		}

		#region Складские остатки
		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set {
				if(SetField(ref warehouse, value)) {
					FillInStock();
					OnPropertyChanged(nameof(Items));
				} 
			}
		}
		private List<Warehouse> warehousesList = new List<Warehouse>();
		public virtual List<Warehouse> WarehousesList {
			get => warehousesList;
			set => SetField(ref warehousesList, value);
		}
		private bool isNullWearPercent;
		public virtual bool IsNullWearPercent {
			get => isNullWearPercent;
			set {
				if(SetField(ref isNullWearPercent, value)) {
					FillInStock();
					OnPropertyChanged(nameof(Items));
				}
			}
		}
		private void FillInStock() {
			if(Warehouse.Id != -1)
				StockBalanceModel.Warehouse = Warehouse;
			else
				StockBalanceModel.Warehouse = null;
			var allNomenclatures = Entity.Items.Select(x => x.Nomenclature).Distinct().ToList();
			StockBalanceModel.AddNomenclatures(allNomenclatures);
			foreach(var item in Entity.Items) {
				item.InStock = StockBalanceModel.GetShipmentItemInStock(item.StockPosition, IsNullWearPercent);
			}
		}
		#endregion
		#region Валидация, сохранение и печать

		public override bool Save() {
			logger.Info ("Запись документа...");
			
			logger.Info("Валидация...");
			if(!Validate()) {
				logger.Warn("Валидация не пройдена, сохранение отменено.");
				return false;
			} else 
				logger.Info("Валидация пройдена.");
			
			logger.Info ("Проверка на дубли");
			string duplicateMessage = "";
			foreach(var duplicate in Entity.Items.GroupBy(x => x.StockPosition).Where(x => x.Count() > 1)) {
				duplicateMessage += $"- {duplicate.First().StockPosition.Title} указано " +
				                    $"{NumberToTextRus.FormatCase(duplicate.Count(), "{0} раз", "{0} раза", "{0} раз")}" 
				                    + $", общим количеством {duplicate.Sum(x=>x.Requested)} \n";
			}
			if(!String.IsNullOrEmpty(duplicateMessage) && !interactive.Question($"В документе есть повторяющиеся позиции:\n{duplicateMessage}\n Сохранить документ?"))
				return false;

			Entity.FullOrdered = Items.All(i => i.Ordered >= i.Requested);
			
			if(Entity.Id == 0) 
				Entity.CreationDate = DateTime.Today;
			UoWGeneric.Save ();
			logger.Info ("Документ сохранён.");
			return true;
		}
		public void Print() 
		{
			if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = Entity.Title,
				Identifier = "Documents.ShipmentSheet",
				Parameters = new Dictionary<string, object> {
					{ "shipment_id",  Entity.Id },
					{ "printPromo", featuresService.Available(WorkwearFeature.PrintPromo)},
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion
	}
}
