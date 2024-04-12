using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
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
			this.uow = unitOfWorkFactory?.CreateWithoutRoot() ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.node = node ?? throw new ArgumentNullException(nameof(node));
			WithoutBarcodesAmount = node.Amount - AllBarcodesAmount;
			Title = node.NomenclatureName;
			Labels = uow.Session.QueryOver<Barcode>()
				.Select(b => b.Label)
				.Where(b => b.Label != null)
				.List<string>()
				.Distinct()
				.ToList();
		}

		#region View Properties
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
		
		public int WithoutBarcodesAmount { get; }
		
		public int AllBarcodesAmount => barcodeService.GetAllBarcodesAmount(uow, node.NomeclatureId);

		public int StockBarcodeAmount => barcodeService.GetStockBarcodesAmount(uow, node.NomeclatureId);

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
			IList<Barcode> barcodes = barcodeService.CreateBarcodesInWarehouse(uow, warehouse, stockPosition, Label, SelectedAmount);
			uow.Commit();
			
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
