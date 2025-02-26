using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Barcodes;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Impl;

namespace Workwear.ViewModels.Stock.Widgets 
{
	public  class StockReleaseBarcodesViewModel : WindowDialogViewModelBase 
	{
		private readonly IUnitOfWork uow;
		private readonly BarcodeService barcodeService;
		private readonly StockBalanceJournalNode node;

		public StockReleaseBarcodesViewModel(INavigationManager navigation, IUnitOfWorkFactory unitOfWorkFactory,
			BarcodeService barcodeService, IOverNormFactory overNormFactory, StockBalanceJournalNode node, Warehouse warehouse = null) : base(navigation) 
		{
			this.uow = unitOfWorkFactory?.CreateWithoutRoot() ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.node = node ?? throw new ArgumentNullException(nameof(node));

			var nomenclature = uow.GetById<Nomenclature>(node.NomeclatureId);
			Size size = node.SizeId != null ? uow.GetById<Size>((int)node.SizeId) : null;
			Size height = node.HeightId != null ? uow.GetById<Size>((int)node.HeightId) : null;
			
			NomenclatureAmount = node.Amount;
			WithBarcodesAmount = barcodeService.CountAllBarcodes(uow, nomenclature, size, height);
			WithoutBarcodesAmount = node.Amount - WithBarcodesAmount;
			BarcodesInStockAmount = this.barcodeService.CountBalanceInStock(uow, nomenclature, size, height, warehouse);
			
			Title = node.NomenclatureName;
			Description = $"Создать штрихкоды для: {node.NomenclatureName}";
			if (!string.IsNullOrEmpty(node.SizeName)) 
			{
				Description += $" размером {node.SizeName}";
			}
			if (!string.IsNullOrEmpty(node.HeightName)) 
			{
				Description += $" ростом {node.HeightName}";
			}
			
			Labels = uow.Session.QueryOver<Barcode>()
				.Where(b => b.Label != null)
				.SelectList(list => list.SelectGroup(b => b.Label))
				.List<string>();
		}

		#region View Properties

		public string Description { get; }
		
		private int selectedAmount;
		[PropertyChangedAlso(nameof(ConfirmButtonSensetive))]
		public int SelectedAmount 
		{
			get => selectedAmount;
			set => SetField(ref selectedAmount, value);
		}

		private string label = string.Empty;
		[PropertyChangedAlso(nameof(ConfirmButtonSensetive))]
		public string Label 
		{
			get => label;
			set => SetField(ref label, value);
		}

		public int NomenclatureAmount { get; set; }
		
		public int WithoutBarcodesAmount { get; }

		public int WithBarcodesAmount { get; }

		public int BarcodesInStockAmount { get; }

		public IList<string> Labels { get; }
		#endregion

		#region Sensetive
		public bool ConfirmButtonSensetive => IsValidForm();
		#endregion
		
		#region Commands
		public void CreateBarcodes() 
		{
			if (!IsValidForm()) return;
 			Warehouse warehouse = uow.GetById<Warehouse>(node.WarehouseId);
			StockPosition stockPosition = node.GetStockPosition(uow);
			
			IEnumerable<Barcode> barcodes = barcodeService.CreateBarcodesInStock(uow, warehouse, stockPosition, SelectedAmount, Label);
			//overNormFactory.CreateSubstitutionBarcodes(uow, warehouse.Id, stockPosition, SelectedAmount, Label);
			
			ReportInfo reportInfo = new ReportInfo() 
			{
				Title = "Штрихкод",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> 
				{
					{ "barcodes", barcodes.Select(x => x.Id).ToList() }
				}
			};
			
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
			Close(false, CloseSource.Self);
		}
		#endregion

		#region Private Methods

		private bool IsValidForm() 
		{
			return SelectedAmount > 0 && SelectedAmount <= WithoutBarcodesAmount && !string.IsNullOrWhiteSpace(Label);
		}

		#endregion
	}
}
