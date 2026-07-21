using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.SqlCommand;
using NLog;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock {
	public class BarcodingViewModel : EntityDialogViewModelBase<Barcoding>, IDialogDocumentation {
		public BarcodingViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, 
			IUserService userService,
			IInteractiveService interactive,
			ILifetimeScope autofacScope,
			BarcodeService barcodeService,
			FeaturesService featuresService,
			StockRepository stockRepository,
			Warehouse warehouse = null,
			StockBalanceJournalNode  stockBalanceJournalNode = null,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.interactive = interactive;
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
				
			var builder = new CommonEEVMBuilderFactory<Barcoding>(this, Entity, UoW, NavigationManager, autofacScope);
			EntryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			
			if(Entity.Id == 0) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info($"Создание нового документа маркировки");
				Entity.Warehouse = warehouse;
				if(Entity.Warehouse == null)
					Entity.Warehouse = stockRepository.GetDefaultWarehouse(featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			} else {
				autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
				loadBarcodes();
			}
			
			if(stockBalanceJournalNode != null)
				OpenReleaseBarcodesWindow(Entity.Warehouse,stockBalanceJournalNode);
		}
		
		private readonly IInteractiveService interactive; 
		private readonly FeaturesService featuresService;
		private readonly BarcodeService barcodeService;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public readonly EntityEntryViewModel<Warehouse> EntryWarehouseViewModel;

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#barcoding-document");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Для View
		public bool SensitiveDocNumber => !AutoDocNumber;
		public virtual bool OwnersVisible => featuresService.Available(WorkwearFeature.Owners);
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}

		public string DocNumberText {
			get => AutoDocNumber ? (Entity.Id == 0 ? "авто" : Entity.Id.ToString()) : Entity.DocNumberText;
			set { 
				if(!AutoDocNumber) 
					Entity.DocNumber = value; 
			}
		}

		public bool SensitiveWarehouse => !Entity.Items.Any(); //Если попросят возможность смены, надо дусмть как аккуратно менять в штрихкодах 
		
		#endregion
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}

		public void DeleteFromItem(BarcodingItem item, Barcode barcode = null) {
			if(item.Id > 0) {
				var deletedBarcodeOperations = item.Barcodes.SelectMany(b => b.BarcodeOperations
					        .Where(o => o.WarehouseOperation.Id == item.OperationReceipt.Id)
					        .Where(o => barcode == null || o.Barcode.Id == barcode.Id))
					.ToList();
				foreach(var operationBarcode in deletedBarcodeOperations)
					UoW.Delete(operationBarcode);
				if(deletedBarcodeOperations.Any()) {
					item.OperationExpense.Amount -= deletedBarcodeOperations.Count;
					item.OperationReceipt.Amount -= deletedBarcodeOperations.Count;
					if(barcode == null)
						item.Barcodes.Clear();
					else
						item.Barcodes.Remove(barcode);
				}
				if(item.OperationExpense.Amount == 0)
					UoW.Delete(item);
			}
			if(item.OperationExpense.Amount == 0)
				Entity.RemoveItem(item);
			OnPropertyChanged(nameof(SensitiveWarehouse));
			CalculateTotal();
		}
		
		public void AddItems() {
			if(Entity.Warehouse == null) {
				interactive.ShowMessage(ImportanceLevel.Warning, "Выберите склад.");
				return;
			}

			var selectJournal = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>
				(this,
					OpenPageOptions.AsSlave,
					addingRegistrations: builder => {
						builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
							filter => {
								filter.ShowNegativeBalance = false;
								filter.ShowWithBarcodes = true; 
								filter.CanChangeShowWithBarcodes = false; 
								filter.CanChooseAmount = true;
								filter.AddAmount = AddedAmount.All;
								filter.Warehouse = Entity.Warehouse;
								filter.SensetiveWarehouse = false;
							});
					});
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
			selectJournal.ViewModel.OnSelectResult += (s, e) =>
				OpenReleaseBarcodesWindow(
					((StockBalanceJournalViewModel)s).Filter.Warehouse,
					e.GetSelectedObjects<StockBalanceJournalNode>().First());
		}

		private void OpenReleaseBarcodesWindow(Warehouse warehouse, StockBalanceJournalNode node) {
			var widget = NavigationManager.OpenViewModel<StockReleaseBarcodesViewModel, StockBalanceJournalNode, Warehouse>
				(this, node, warehouse);
			widget.PageClosed += LoadFromWidget;
		}

		private void LoadFromWidget(object sender, PageClosedEventArgs e) {
			if(e.CloseSource == CloseSource.Save) {
				var widgetVm = (sender as IPage<StockReleaseBarcodesViewModel>)?.ViewModel;
				if(widgetVm == null)
					throw new ArgumentNullException(nameof(StockReleaseBarcodesViewModel));
				
				var stockPosition = new StockPosition(
					UoW.GetById<Nomenclature>(widgetVm.balanceNode.NomenclatureId),
					widgetVm.balanceNode.WearPercent,
					widgetVm.balanceNode.SizeId != null ? UoW.GetById<Size>((int)widgetVm.balanceNode.SizeId) : null,
					widgetVm.balanceNode.HeightId != null ? UoW.GetById<Size>((int)widgetVm.balanceNode.HeightId) : null,
					widgetVm.balanceNode.OwnerId != null ? UoW.GetById<Owner>((int)widgetVm.balanceNode.OwnerId) : null
				);
				
				var warehouse = UoW.GetById<Warehouse>(widgetVm.balanceNode.WarehouseId);
				var operationExpanse = new WarehouseOperation() {
					StockPosition = stockPosition,
					Amount = widgetVm.SelectedAmount,
					ExpenseWarehouse = warehouse,
					OperationTime = Entity.Date
				};
				
				BarcodeOperation barcodeOperationAlias = null;
				WarehouseOperation warehouseOperationAlias = null;
				var lastKitNumber = UoW.Session.QueryOver(() => barcodeOperationAlias)
					.JoinAlias(() => barcodeOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
					.Where(() => warehouseOperationAlias.Nomenclature.Id == stockPosition.Nomenclature.Id)
					.Where(() => warehouseOperationAlias.ReceiptWarehouse.Id == warehouse.Id)
					.SelectList(list => list.SelectMax(() => barcodeOperationAlias.KitNumber))
					.SingleOrDefault<int?>() ?? 0;
				
				var operationReceipt = new WarehouseOperation() {
					StockPosition = stockPosition,
					ReceiptWarehouse = warehouse,
					Amount = widgetVm.SelectedAmount
				};
				
				IList<Barcode> barcodes = barcodeService.Create(
					UoW,
					widgetVm.SelectedAmount,
					stockPosition.Nomenclature,
					stockPosition.WearSize,
					stockPosition.Height,
					widgetVm.Label
					);
				
				var barcodeOperations = new List<BarcodeOperation>();
				foreach(Barcode barcode in barcodes) {
					barcode.Label = widgetVm.Label;
					BarcodeOperation barcodeOperation = new BarcodeOperation() {
						Barcode = barcode,
						WarehouseOperation = operationReceipt,
						//Нумерация по порядку в рамках склада и номенклатуры
						KitNumber = ++lastKitNumber 
					};
					barcodeOperations.Add(barcodeOperation);
					UoW.Save(barcodeOperation, false);
				}
				Entity.AddItem(operationExpanse, operationReceipt, barcodes);
				Save();
				OnPropertyChanged(nameof(SensitiveWarehouse));
				
				if(widgetVm.NeedPrint) 
					PrintBarcodes(barcodes);
			}
		}

		private void PrintBarcodes(IList<Barcode> barcodes) {
			ReportInfo reportInfo = new ReportInfo() {
				Title = "Штрихкод",
				Identifier = "Barcodes.Barcode",
				Parameters = new Dictionary<string, object> {
					{ "barcodes", barcodes.Select(x => x.Id).ToList() }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}

		private void loadBarcodes() {
			Barcode barcodeAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			BarcodingItem barcodingItemAlias = null;
			
			var barcodes = UoW.Session.QueryOver<Barcode>(() => barcodeAlias)
				.Left.JoinAlias(x => x.BarcodeOperations, () => barcodeOperationAlias)
				.Fetch(SelectMode.Fetch, x => x.BarcodeOperations)
				.JoinAlias(() => barcodeOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.Fetch(SelectMode.Fetch, () => barcodeOperationAlias.WarehouseOperation)
				.JoinEntityAlias(() => barcodingItemAlias,
					() => barcodingItemAlias.OperationReceipt.Id == warehouseOperationAlias.Id,
					JoinType.LeftOuterJoin)
				.Where(() => barcodingItemAlias.Document.Id == Entity.Id)
				.List();

			foreach(var item in Entity.Items) {
				item.Barcodes = barcodes.Where(b => b.BarcodeOperations
					.Any(bo => bo?.WarehouseOperation.Id == item.OperationReceipt.Id))
					.ToList();
			}
		}

		public override bool Save() {
			logger.Info ("Запись документа...");
			
			if(AutoDocNumber)
					Entity.DocNumber = null;
			
			UoW.Save(Entity);
			foreach(var item in Entity.Items) {
				UoW.Save(item.OperationExpense);
				UoW.Save(item.OperationReceipt);
				UoW.Save(item);
			}

			if(!base.Save()) {
				logger.Info("Не Ок.");
				return false;
			}

			logger.Info ("Ok");
			return true;
		}

		private void CalculateTotal() {
			Total = $"Позиций в документе: {Entity.Items.Count}, Общее количество: {Entity.Items.Sum(i => i.Amount)}";
		}

		public void PrintBarcodesforItems(IList<BarcodingItem> items = null) {
			if(items == null)
				items = Entity.Items;
			PrintBarcodes(items.SelectMany(i => i.Barcodes).ToList());
		}
////1289 не реализовано и не факт, что нужно
		public void Print() {
			if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = String.Format("Документ маркировки №{0}", Entity.DocNumber ?? Entity.Id.ToString()),
				Identifier = "Documents.BarcodingSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};
		}
	}
}
