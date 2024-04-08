using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Barcodes;

namespace Workwear.ViewModels.Stock.Widgets 
{
	public class StockReleaseBarcodesViewModel : WindowDialogViewModelBase 
	{
		private readonly IUnitOfWork uow;
		private readonly BarcodeService barcodeService;
		private readonly StockBalanceJournalNode node;

		public StockReleaseBarcodesViewModel(INavigationManager navigation, IUnitOfWorkFactory unitOfWorkFactory,
			BarcodeService barcodeService, StockBalanceJournalNode node) : base(navigation) 
		{
			uow = unitOfWorkFactory?.CreateWithoutRoot() ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.node = node ?? throw new ArgumentNullException(nameof(node));
			int barcodesAmount = barcodeService.GetBarcodesCount(uow, node.NomeclatureId);
			MaxAmount = node.Amount - barcodesAmount;
			Title = node.NomenclatureName;
		}

		#region View Properties
		public int MaxAmount { get; }

		private int selectedAmount;
		[PropertyChangedAlso(nameof(ConfirmButtonSensetive))]
		public int SelectedAmount 
		{
			get => selectedAmount;
			set => SetField(ref selectedAmount, value);
		}

		#endregion

		#region Sensetive
		public bool ConfirmButtonSensetive => SelectedAmount > 0 && SelectedAmount <= MaxAmount;
		#endregion
		
		#region Commands
		public void CreateBarcodes() 
		{
			Warehouse warehouse = uow.GetById<Warehouse>(node.WarehouseId);
			StockPosition stockPosition = node.GetStockPosition(uow);
			barcodeService.CreateBarcodesInWarehouse(uow, warehouse, stockPosition, SelectedAmount);
			uow.Commit();
			Close(false, CloseSource.Self);
		}
		#endregion
	}
}
