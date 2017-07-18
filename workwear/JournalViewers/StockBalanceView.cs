using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using QSTDI;
using workwear.DTO;

namespace workwear
{
	public partial class StockBalanceView : TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		List<StockBalanceItems> listedItems;
		GenericObservableFilterListView<StockBalanceItems> filter;

		public StockBalanceView ()
		{
			this.Build ();
			TabName = "Складские остатки";

			ytreeviewStockBalance.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<StockBalanceItems> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.NomenclatureName)
				.AddColumn ("Размер").AddTextRenderer (e => e.Size)
				.AddColumn ("Рост").AddTextRenderer (e => e.Growth)
				.AddColumn ("Количество").AddTextRenderer (e => e.AmountText)
				.AddColumn ("Средняя стоимость").AddTextRenderer (e => e.AvgCostText)
				.AddColumn ("Среднее состояние").AddTextRenderer (e => e.AvgLifeText)
				.Finish ();
			ytreeviewStockBalance.ShowAll ();
			RefreshView();
		}

		protected void OnButtonSearchCleanClicked (object sender, EventArgs e)
		{
			entryStockBalanceSearch.Text = String.Empty;
		}

		void RefreshView()
		{
			QSMain.CheckConnectionAlive ();
			logger.Info ("Запрос складских остатков...");
			try {
				string sql = "SELECT stock_income_detail.nomenclature_id, " +
					"SUM(stock_income_detail.quantity - ifnull(spent.count, 0)) as quantity, " +
					"nomenclature.name as nomenclature, " +
					"SUM(stock_income_detail.life_percent * (stock_income_detail.quantity - ifnull(spent.count, 0)))/SUM(stock_income_detail.quantity - ifnull(spent.count, 0))as avg_life, " +
					"nomenclature.size, nomenclature.growth, measurement_units.name as unit, " +
					"SUM(stock_income_detail.cost * (stock_income_detail.quantity - ifnull(spent.count, 0)))/SUM(stock_income_detail.quantity - ifnull(spent.count, 0)) as avgcost " +
					"FROM stock_income_detail " +
					"LEFT JOIN (SELECT id, SUM(count) as count FROM " +
					"(SELECT stock_expense_detail.stock_income_detail_id as id, stock_expense_detail.quantity as count FROM stock_expense_detail " +
					"UNION ALL " +
					"SELECT stock_write_off_detail.stock_income_detail_id as id, stock_write_off_detail.quantity as count FROM stock_write_off_detail WHERE stock_income_detail_id IS NOT NULL" +
					") as table1 " +
					"GROUP BY id) as spent ON spent.id = stock_income_detail.id " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_income_detail.nomenclature_id " +
					"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
					"LEFT JOIN measurement_units ON measurement_units.id = item_types.units_id " +
					"WHERE spent.count IS NULL OR spent.count < stock_income_detail.quantity " +
					"GROUP BY stock_income_detail.nomenclature_id";
				MySqlCommand cmd = new MySqlCommand (sql, QSMain.connectionDB);

				listedItems = new List<StockBalanceItems>();

				using(MySqlDataReader rdr = cmd.ExecuteReader ())
				{
					while (rdr.Read ()) {
						listedItems.Add (new StockBalanceItems{
							NomenclatureId = rdr.GetInt32 ("nomenclature_id"),
							NomenclatureName = rdr.GetString ("nomenclature"),
							UnitsName = DBWorks.GetString (rdr, "unit", String.Empty),
							Amount = rdr.GetInt32 ("quantity"),
							AvgCost = rdr.GetDecimal ("avgcost"),
							AvgLife = rdr.GetDecimal ("avg_life"),
							Size = DBWorks.GetString (rdr, "size", String.Empty),
							Growth = DBWorks.GetString (rdr, "growth", String.Empty)
						});
					}
				}
				filter = new GenericObservableFilterListView<StockBalanceItems>(listedItems);
				filter.IsVisibleInFilter += Filter_IsVisibleInFilter;
				ytreeviewStockBalance.ItemsDataSource = filter;
				logger.Info ("Ok");
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog("Ошибка складских остатков!", logger, ex);
			}
		}

		bool Filter_IsVisibleInFilter (object aObject)
		{
			if (entryStockBalanceSearch.Text == "")
				return true;

			var item = aObject as StockBalanceItems;

			return item.NomenclatureName.IndexOf (entryStockBalanceSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1;
		}

		protected void OnEntryStockBalanceSearchChanged (object sender, EventArgs e)
		{
			filter.Refilter ();
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			RefreshView();
		}
	}
}

