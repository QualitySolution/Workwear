using System;
using QS.Dialog.Gtk;

namespace workwear
{
	public partial class StockBalanceView : TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public StockBalanceView ()
		{
			this.Build ();
			TabName = "Складские остатки";

			ytreeviewStockBalance.RepresentationModel = new ViewModel.StockBalanceVM(ViewModel.StockBalanceVMMode.DisplayAll, ViewModel.StockBalanceVMGroupBy.NomenclatureId);
		}

		protected void OnButtonSearchCleanClicked (object sender, EventArgs e)
		{
			entryStockBalanceSearch.Text = String.Empty;
		}

		protected void OnEntryStockBalanceSearchChanged (object sender, EventArgs e)
		{
			ytreeviewStockBalance.RepresentationModel.SearchString = entryStockBalanceSearch.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			ytreeviewStockBalance.RepresentationModel.UpdateNodes();
		}
	}
}

