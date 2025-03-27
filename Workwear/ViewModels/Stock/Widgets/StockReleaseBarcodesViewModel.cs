using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Barcodes;

namespace Workwear.ViewModels.Stock.Widgets 
{
	public  class StockReleaseBarcodesViewModel : WindowDialogViewModelBase 
	{
		private readonly BarcodeService barcodeService;
		public readonly StockBalanceJournalNode balanceNode;

		public StockReleaseBarcodesViewModel(
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			BarcodeService barcodeService,
			StockBalanceJournalNode node,
			Warehouse warehouse
			) : base(navigation) 
		{
			var uow = unitOfWorkFactory?.CreateWithoutRoot() ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.balanceNode = node ?? throw new ArgumentNullException(nameof(node));

			StockPosition stockPosition = node.GetStockPosition(uow);
////1289			
var nomenclature = stockPosition.Nomenclature;
Size size = stockPosition.WearSize;
Size height = stockPosition.Height;
			
			NomenclatureAmount = node.Amount;
			WithBarcodesAmount = barcodeService.CountAllBarcodes(uow, stockPosition);
			WithoutBarcodesAmount = node.Amount - WithBarcodesAmount;
			BarcodesInStockAmount = this.barcodeService.CountBalanceInStock(uow, nomenclature, size, height, warehouse);
			WarehouseId = warehouse.Id;
				
			Title = "Создать штрихкоды";
			Description = $"Создать штрихкоды для: {node.NomenclatureName}";
			
			if (!string.IsNullOrEmpty(node.SizeName)) 
				Description += $" размером {node.SizeName}";
			if (!string.IsNullOrEmpty(node.HeightName)) 
				Description += $" ростом {node.HeightName}";
			
			Labels = uow.Session.QueryOver<Barcode>()
				.Where(b => b.Label != null)
				.SelectList(list => list.SelectGroup(b => b.Label))
				.List<string>();
		}

		#region View Properties
		public string Description { get; }
		
		private int selectedAmount;
		[PropertyChangedAlso(nameof(ConfirmButtonSensetive))]
		public int SelectedAmount {
			get => selectedAmount;
			set => SetField(ref selectedAmount, value);
		}

		private string label = string.Empty;
		[PropertyChangedAlso(nameof(ConfirmButtonSensetive))]
		public string Label {
			get => label;
			set => SetField(ref label, value);
		}

		public int NomenclatureAmount { get; set; }
		public int WithoutBarcodesAmount { get; }
		public int WithBarcodesAmount { get; }
		public int BarcodesInStockAmount { get; }
		public bool NeedPrint { get; set; } = false;
		public int WarehouseId { get; }

		public IList<string> Labels { get; }
		#endregion

		#region Sensetive
		public bool ConfirmButtonSensetive => SelectedAmount > 0 && SelectedAmount <= WithoutBarcodesAmount;
		#endregion
		
		#region Commands

		public void CreateAndPrint() {
			NeedPrint = true;
			Close(false, CloseSource.Save);
		}
		public void CreateAndClose() => Close(false, CloseSource.Save);
		
		#endregion
	}
}
