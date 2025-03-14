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
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock {
	public class BarcodingViewModel : EntityDialogViewModelBase<Barcoding> {
		public BarcodingViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, 
			IUserService userService,
			IInteractiveService interactive,
			ILifetimeScope autofacScope,
			BarcodeService barcodeService,
			BaseParameters baseParameters,
			FeaturesService featuresService,
			StockRepository stockRepository,
			Warehouse warehouse = null,
			StockBalanceJournalNode  stockBalanceJournalNode = null,
			IValidator validator = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator) {
			this.interactive = interactive;
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
				
			var entryBuilder = new CommonEEVMBuilderFactory<Barcoding>(this, Entity, UoW, navigation) {
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope))
			};
			
			var builder = new CommonEEVMBuilderFactory<Barcoding>(this, Entity, UoW, NavigationManager, autofacScope);
			EntryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			
			if(Entity.Id == 0) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info($"Создание Нового документа маркировки");
				Entity.Warehouse = warehouse;
				if(Entity.Warehouse == null)
					Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			} else {
				autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
				 loadBarcodes();
			}
			
			if(stockBalanceJournalNode != null)
				OpenReleaseBarcodesWindow(Entity.Warehouse,stockBalanceJournalNode);
		}
//1289		
		private IInteractiveService interactive; 
//1289
		private BaseParameters baseParameters;
		private FeaturesService featuresService;
		private readonly BarcodeService barcodeService;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public readonly EntityEntryViewModel<Warehouse> EntryWarehouseViewModel;

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

		#endregion
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}

		public void DeleteItem(BarcodingItem item) {
			if(item.Id > 0) {
////1289 Пока не придумал, как задать через мапинг, возможно сделать через удалялку
				foreach(var operationBarcode in
				        item.Barcodes.SelectMany(b => b.BarcodeOperations
					        .Where(o => o.WarehouseOperation.Id == item.OperationReceipt.Id))) 
					UoW.Delete(operationBarcode);
				UoW.Delete(item);
			}
			Entity.RemoveItem(item);
			CalculateTotal();
		}
		
		public void AddItems() {
			var selectJournal = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.Warehouse = Entity.Warehouse; 
			selectJournal.ViewModel.Filter.SensetiveWarehouse = false;
			selectJournal.ViewModel.Filter.CanChooseAmount = true;
			selectJournal.ViewModel.Filter.AddAmount = AddedAmount.All;
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
					UoW.GetById<Nomenclature>(widgetVm.balanceNode.NomeclatureId),
					widgetVm.balanceNode.WearPercent,
					widgetVm.balanceNode.SizeId != null ? UoW.GetById<Size>((int)widgetVm.balanceNode.SizeId) : null,
					widgetVm.balanceNode.HeightId != null ? UoW.GetById<Size>((int)widgetVm.balanceNode.HeightId) : null,
					widgetVm.balanceNode.OwnerId != null ? UoW.GetById<Owner>((int)widgetVm.balanceNode.OwnerId) : null
				);
				
				var warehouse = UoW.GetById<Warehouse>(widgetVm.balanceNode.WarehouseId);
				var operationExpance = new WarehouseOperation() {
					StockPosition = stockPosition,
					Amount = widgetVm.SelectedAmount,
					ExpenseWarehouse = warehouse,
					OperationTime = Entity.Date
				};
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

				foreach(Barcode barcode in barcodes) {
					barcode.Label = widgetVm.Label;
					BarcodeOperation barcodeOperation = new BarcodeOperation() {
						Barcode = barcode,
						WarehouseOperation = operationReceipt
					};
					UoW.Save(barcodeOperation, false);
				}
				Entity.AddItem(operationExpance, operationReceipt, barcodes);
				Save();
				
				if(widgetVm.NeedPrint) 
					PrintBarcodes(barcodes);
			}
		}

		private void PrintBarcodes(IList<Barcode> barcodes) {
			ReportInfo reportInfo = new ReportInfo() {
				Title = "Штрихкод",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> {
					{ "barcodes", barcodes.Select(x => x.Id).ToList() }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}

////1289
		private void LoadItems(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var addedAmount = ((StockBalanceJournalViewModel)sender).Filter.AddAmount;
			
			foreach (var node in e.GetSelectedObjects<StockBalanceJournalNode>())
				Entity.AddItem(node.GetStockPosition(UoW),
					((StockBalanceJournalViewModel)sender).Filter.Warehouse, 
					addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : node.Amount));
			CalculateTotal();
		}

		private void loadBarcodes() {
			Barcode barcodeAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			BarcodingItem barcodingItemAlias = null;

			var barcodes = UoW.Session.QueryOver<Barcode>(() => barcodeAlias)
				.Left.JoinAlias(x => x.BarcodeOperations, () => barcodeOperationAlias)
				.Left.JoinAlias(() => barcodeOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.JoinEntityAlias(() => barcodingItemAlias, () => barcodingItemAlias.OperationReceipt.Id == warehouseOperationAlias.Id,
					JoinType.LeftOuterJoin)
				.Where(() => barcodingItemAlias.Document.Id == Entity.Id)
				.List();

			foreach(var item in Entity.Items) {
				item.Barcodes = barcodes.Where(b => b.BarcodeOperations
					.Any(bo => bo?.WarehouseOperation.Id == item.OperationReceipt.Id));
			}
		}

		public override bool Save() {
			logger.Info ("Запись документа...");
			
			if(AutoDocNumber)
					Entity.DocNumber = null;
			
			UoW.Save(Entity);
			foreach(var item in Entity.Items) {
				UoW.Save(item.OperationExpence);
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
////1289		
		private void CalculateTotal() {
			Total = $"Позиций в документе: {Entity.Items.Count}";
		}

		public void PrintBarcodesforItems(IList<BarcodingItem> items = null) {
			if(items == null)
				items = Entity.Items;
			PrintBarcodes(items.SelectMany(i => i.Barcodes).ToList());
		}

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
