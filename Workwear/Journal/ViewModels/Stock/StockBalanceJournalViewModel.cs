using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Dapper;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.Search;
using QS.Utilities;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock.Widgets;
using ArgumentNullException = System.ArgumentNullException;

namespace workwear.Journal.ViewModels.Stock
{
	/// <summary>
	/// Stock balance journal view model. Для подробного отображения баланса склада
	/// </summary>
	public class StockBalanceJournalViewModel : JournalViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public bool ShowSummary;
		private readonly IConnectionFactory connectionFactory;
		public readonly FeaturesService FeaturesService;

		public StockBalanceFilterViewModel Filter { get; private set; }

		public StockBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IConnectionFactory connectionFactory,
			FeaturesService featuresService) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			this.FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			JournalFilter = Filter = autofacScope.Resolve<StockBalanceFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new AnyDataLoader<StockBalanceJournalNode>(GetNodes);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation), typeof(Nomenclature));
			TabName = "Остатки по складу " + 
			          (featuresService.Available(WorkwearFeature.Warehouses) ? Filter.Warehouse?.Name : "");

			Filter.PropertyChanged += (sender, e) => 
				TabName = "Остатки по складу " + 
				          (featuresService.Available(WorkwearFeature.Warehouses) ? Filter.Warehouse?.Name : "");
		}

		private IList<StockBalanceJournalNode> GetNodes(CancellationToken cancellation) {
			using(var connection = connectionFactory.OpenConnection()) {
				
				
				var conductions = new List<string>();
				if(Filter.SelectOwner != null)
					switch(Filter.SelectOwner) {
						case (SpecialComboState.All): break; //все
						case (SpecialComboState.Not): conductions.Add("operation.owner_id IS NULL"); break; //без собственника 
						default: conductions.Add($"operation.owner_id = {Filter.SelectOwner.GetId()}"); break;
					}
				
				//Если у нас выключена способность показывать общие по всем складам остатки.
				//Но не указан склад мы должны показывать пустую таблицу.
				//Это заведомо ложное условие.
				if(ShowSummary == false && Filter.Warehouse == null)
					conductions.Add("operation.id = -1");

				if (Filter.ProtectionTools != null && Filter.ProtectionTools.Nomenclatures.Any())
					conductions.Add($"nomenclature.id IN ({string.Join(",", Filter.ProtectionTools.Nomenclatures.Select(x => x.Id))})");
				
				var search = new SqlSearchCriterion(Search)
					.WithLikeMode(LikeMatchMode.UnsignedNumberEqual)
					.By("nomenclature.id")
					.WithLikeMode(LikeMatchMode.StringAnywhere)
					.By("nomenclature.name")
					.By("nomenclature.number")
					.By("sizealias.name")
					.By("heightalias.name")
					.Finish();
				if(!String.IsNullOrEmpty(search))
					conductions.Add(search);

				var sql = @"
SELECT 
    stock.*,
    (SELECT SUM(operation_sub.amount)/DATEDIFF(NOW(), MIN(operation_sub.operation_time))
     FROM operation_warehouse operation_sub
     WHERE operation_sub.nomenclature_id = stock.NomeclatureId
         AND operation_sub.size_id <=> stock.SizeId
         AND operation_sub.height_id <=> stock.HeightId
         AND operation_sub.owner_id <=> stock.OwnerId
         AND operation_sub.wear_percent = stock.WearPercent
       AND (operation_sub.operation_time < @report_date
       AND operation_sub.operation_time >= ADDDATE(@report_date, INTERVAL -1 YEAR ))
       AND NOT (operation_sub.warehouse_expense_id IS NULL)
       ) AS DailyConsumption
    FROM (SELECT nomenclature.id        AS NomeclatureId,
                 nomenclature.name      AS NomenclatureName,
                 nomenclature.number    AS NomenclatureNumber,
                 nomenclature.sex       AS Sex,
                 unit.name              AS UnitsName,
                 sizealias.id           AS SizeId,
                 sizealias.name         AS SizeName,
                 heightalias.id         AS HeightId,
                 heightalias.name       AS HeightName,
                 operation.wear_percent AS WearPercent,
                 (SUM(IF((operation.warehouse_receipt_id IS NOT NULL AND
                          (@all_warehouse OR operation.warehouse_receipt_id = @warehouse_id)),
                         operation.amount, 0))
                     - SUM(IF((operation.warehouse_expense_id IS NOT NULL AND
                               (@all_warehouse OR operation.warehouse_expense_id = @warehouse_id)),
                              operation.amount, 0))
	                 )                      AS Amount,
	             owners.id                  AS OwnerId,
	             owners.name                AS OwnerName,
	             nomenclature.use_barcode   AS UseBarcode,	             
             	 operation.warehouse_receipt_id AS WarehouseId,
	             nomenclature.sale_cost     AS SaleCost
          FROM nomenclature
                   JOIN operation_warehouse AS operation
                             on (operation.operation_time < ADDDATE(@report_date, INTERVAL 1 DAY)
                                 AND operation.nomenclature_id = nomenclature.id
                                 AND (@all_warehouse
                                     OR operation.warehouse_receipt_id = @warehouse_id
                                     OR operation.warehouse_expense_id = @warehouse_id))
                   LEFT JOIN sizes sizealias ON operation.size_id = sizealias.id
                   LEFT JOIN sizes heightalias ON operation.height_id = heightalias.id
                   LEFT JOIN owners ON operation.owner_id = owners.id
                   LEFT JOIN item_types ON nomenclature.type_id = item_types.id
                   LEFT JOIN measurement_units unit ON item_types.units_id = unit.id";
				if(conductions.Any())
					sql += " WHERE " + string.Join(" AND ", conductions);
				var having = Filter.ShowNegativeBalance ? "!=" : ">";
				sql += $@"
          GROUP BY nomenclature.id, operation.size_id, operation.height_id, operation.owner_id, operation.wear_percent
          HAVING Amount {having} 0
          ORDER BY nomenclature.name, sizealias.name, heightalias.name, nomenclature.sex, owners.name, operation.wear_percent
         ) AS stock";

				var onDate = Filter.Date.AddDays(1);
				// Если дата не указана, то берем склад на сегодня. Добавляем 10 лет, чтобы исключить падение, так как мы от этой даты отнимаем год.
				if(onDate <= default(DateTime).AddYears(10))
					onDate = DateTime.Today.AddDays(1);
				
				logger.Debug(sql);
				var result = connection.Query<StockBalanceJournalNode>(sql, new {
					report_date = onDate,
					warehouse_id = Filter.Warehouse?.Id,
					all_warehouse = Filter.Warehouse == null
				}).ToList();
				logger.Debug($"Получено {result.Count} складских позиций.");
				return result;
			}
		}
//1289		
		/*
		private IQueryOver<WarehouseOperation> BarcodesStockBalanceQuery(IUnitOfWork uow) 
		{
			StockBalanceJournalNode resultAlias = null;
			
			Barcode bSubAlias = null;
			BarcodeOperation boSubAlias = null;
			
			Barcode bSub1Alias = null;
			BarcodeOperation boSubAlias1 = null;
			OverNormOperation oonSubAlias1 = null;
			WarehouseOperation woSubAlias1 = null;
			
			BarcodeOperation boAlias = null;
			WarehouseOperation woAlias = null;
			OverNormOperation onAlias = null;
			Barcode bAlias = null;
			Nomenclature nAlias = null;
			ItemsType itAlias = null;
			MeasurementUnits unitsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			Owner ownerAlias = null;
			
			var subQuery = QueryOver.Of(() => boSubAlias)
				.JoinAlias(() => boSubAlias.Barcode, () => bSubAlias, JoinType.InnerJoin)
				.Select(Projections.Group(() => bSubAlias.Id))
				.Where(Restrictions.Eq(Projections.Count(() => bSubAlias.Id), 1))
				.Where(x => x.Barcode == boAlias.Barcode);
				
			var subQuery1 = QueryOver.Of(() => boSubAlias1)
				.JoinAlias(() => boSubAlias1.Barcode, () => bSub1Alias)
				.JoinAlias(() => boSubAlias1.OverNormOperation, () => oonSubAlias1)
				.JoinAlias(() => oonSubAlias1.WarehouseOperation, () => woSubAlias1)
				.Where(() => boSubAlias1.OverNormOperation != null)
				.SelectList(list => list
					.SelectGroup(() => bSub1Alias.Id)
					.SelectMax(() => oonSubAlias1.OperationTime))
				.Where(x => x.Barcode == boAlias.Barcode && onAlias.OperationTime > oonSubAlias1.OperationTime);
			
			var queryStock = uow.Session.QueryOver(() => woAlias)
				.JoinEntityAlias(() => onAlias, () => onAlias.WarehouseOperation.Id == woAlias.Id, JoinType.RightOuterJoin)
				.JoinEntityAlias(() => boAlias, () => boAlias.OverNormOperation.Id == onAlias.Id, JoinType.RightOuterJoin)
				.JoinAlias(() => boAlias.Barcode, () => bAlias, JoinType.InnerJoin)
				.JoinAlias(() => bAlias.Size, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => bAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => bAlias.Nomenclature, () => nAlias, JoinType.InnerJoin)
				.JoinAlias(() => nAlias.Type, () => itAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => woAlias.Owner, () => ownerAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => itAlias.Units, () => unitsAlias)
				.Where(Restrictions.Disjunction()
					.Add(Subqueries.WhereExists(subQuery))
					.Add(Subqueries.WhereExists(subQuery1)))
				.Where(Restrictions.Or(
					Restrictions.Where(() => boAlias.Warehouse != null && (Filter.Warehouse == null || boAlias.Warehouse == Filter.Warehouse)),
					Restrictions.Where(() =>
						woAlias.ReceiptWarehouse != null && (Filter.Warehouse == null || woAlias.ReceiptWarehouse == Filter.Warehouse)))
				)
				.Where(GetSearchCriterion(
					() => nAlias.Id,
					() => nAlias.Number,
					() => nAlias.Name,
					() => sizeAlias.Name,
					() => heightAlias.Name))
				.SelectList(list => list
					.Select(() => boAlias.Warehouse.Id.Coalesce(woAlias.ReceiptWarehouse.Id)).WithAlias(() => resultAlias.WarehouseId)
					.Select(() => nAlias.Id).WithAlias(() => resultAlias.NomeclatureId)
					.Select(() => nAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => nAlias.Number).WithAlias(() => resultAlias.NomenclatureNumber)
					.Select(() => nAlias.Sex).WithAlias(() => resultAlias.Sex)
					.Select(() => nAlias.UseBarcode).WithAlias(() => resultAlias.UseBarcode)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.SizeName)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.HeightName)
					.Select(() => woAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
					.Select(() => ownerAlias.Id).WithAlias(() => resultAlias.OwnerId)
					.SelectCount(() => bAlias.Id).WithAlias(() => resultAlias.Amount)
					.SelectGroup(() => nAlias.Id).WithAlias(() => resultAlias.Id)
					.SelectGroup(() => sizeAlias.Id).WithAlias(() => resultAlias.SizeId)
					.SelectGroup(() => heightAlias.Id).WithAlias(() => resultAlias.HeightId)
				)
				.OrderBy(() => nAlias.Name).Asc
				.ThenBy(Projections.SqlFunction(
					new SQLFunctionTemplate(
						NHibernateUtil.String,
						"CAST(SUBSTRING_INDEX(?1, '-', 1) AS DECIMAL(5,1))"),
					NHibernateUtil.String,
					Projections.Property(() => sizeAlias.Name))).Asc
				.ThenBy(Projections.SqlFunction(
					new SQLFunctionTemplate(
						NHibernateUtil.String,
						"CAST(SUBSTRING_INDEX(?1, '-', 1) AS DECIMAL(5,1))"),
					NHibernateUtil.String,
					Projections.Property(() => heightAlias.Name))).Asc;
			
			if (Filter.Warehouse != null) 
			{
				queryStock.Where(() => woAlias.ReceiptWarehouse == Filter.Warehouse || boAlias.Warehouse == Filter.Warehouse);
			}
			if (Filter.ProtectionTools != null) 
			{
				queryStock.Where(() 
					=> woAlias.Nomenclature.IsIn(Filter.ProtectionTools.Nomenclatures.ToArray()));
			}
			if (Filter.ItemsType != null) 
			{
				queryStock.Where(() => itAlias.Id == Filter.ItemsType.Id);
			}

			return queryStock.TransformUsing(Transformers.AliasToBean<StockBalanceJournalNode>());
		}
		*/
		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			var updateStatusAction = new JournalAction("Показать движения",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => OpenMovements(selected.Cast<StockBalanceJournalNode>().ToArray())
					);
			NodeActionsList.Add(updateStatusAction);
//1289			
			
			JournalAction releaseBarcodesAction = new JournalAction("Создать штрихкоды",
				(selected) => selected.Any(x => 
					x is StockBalanceJournalNode node && 
					node.UseBarcode && 
					node.Amount > 0),
				(selected) => !Filter.ShowWithBarcodes,
				(selected) => NavigationManager.OpenViewModel<StockReleaseBarcodesViewModel, StockBalanceJournalNode, Warehouse>
						(this, selected.First() as StockBalanceJournalNode, Filter.Warehouse)
//OpenReleaseBarcodesWindow( selected.First() as StockBalanceJournalNode)
			);

			NodeActionsList.Add(releaseBarcodesAction);
		}

		void OpenMovements(StockBalanceJournalNode[] nodes)
		{
			foreach(var node in nodes) {
				var journal = NavigationManager
					.OpenViewModel<StockMovmentsJournalViewModel>(this);
				journal.ViewModel.Filter.SetAndRefilterAtOnce(
					filter => filter.Warehouse = Filter.Warehouse, 
					filter => filter.StockPosition = node.GetStockPosition(journal.ViewModel.UoW));
			}
		}
		private void OpenReleaseBarcodesWindow(StockBalanceJournalNode node) =>
			NavigationManager.OpenViewModel<StockReleaseBarcodesViewModel, StockBalanceJournalNode, Warehouse>
				(this, node, Filter.Warehouse);
		
		public override string FooterInfo {
			get => $"Суммарная стоимость: " +
			       $"{CurrencyWorks.GetShortCurrencyString(DataLoader.Items.Cast<StockBalanceJournalNode>().Sum(x => x.SumSaleCost))} " +
			       $"    Загружено:	" +
			       $"{DataLoader.Items.Count} шт.";
			set { }
		}
	}

	public class StockBalanceJournalNode
	{
		public int NomeclatureId { get; set; }
		public string NomenclatureName { get; set; }
		public string NomenclatureNumber { get; set; }
		public ClothesSex Sex { get; set; }
		public string SexText => Sex.GetEnumShortTitle();
		public string UnitsName { get; set; }
		public string SizeName { get; set; }
		public int? SizeId { get; set; }
		public string HeightName { get; set; }
		public int? HeightId { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }
		public int? OwnerId { get; set; }
		public string OwnerName { get; set; }
		public bool UseBarcode { get; set; }
		public decimal SaleCost { get; set; }
		public double? DailyConsumption { get; set; }

		public string MonthConsumption => $"{DailyConsumption * 30:N1}";

		public int? SupplyDays => (int?)(Amount / DailyConsumption);

		public string Supply => DailyConsumption.HasValue && SupplyDays >= 0 ? NumberToTextRus.FormatCase(SupplyDays.Value, "{0} день", "{0} дня", "{0} дней") : null;

		public string SupplyColor {
			get {
				if(SupplyDays <= 7) return "red";
				if(SupplyDays <= 14) return "orange";
				if(SupplyDays <= 365/2) return "green";
				return "violet";
			}
		}
		public string SaleCostText => SaleCost > 0 ? CurrencyWorks.GetShortCurrencyString (SaleCost) : String.Empty;
		public decimal SumSaleCost => SaleCost > 0 ? SaleCost * Amount : 0;
		public string SumSaleCostText => CurrencyWorks.GetShortCurrencyString (SumSaleCost);
		public string BalanceText => Amount > 0 ? 
			$"{Amount} {UnitsName}" : $"<span foreground=\"red\">{Amount}</span> {UnitsName}";
		public string WearPercentText => WearPercent.ToString("P0");

		public StockPosition GetStockPosition(IUnitOfWork uow) => new StockPosition(
			uow.GetById<Nomenclature>(NomeclatureId), 
			WearPercent, 
			SizeId.HasValue ? uow.GetById<Size>(SizeId.Value) : null, 
			HeightId.HasValue ? uow.GetById<Size>(HeightId.Value) : null,
			OwnerId.HasValue ? uow.GetById<Owner>(OwnerId.Value) : null);
		
		public int WarehouseId { get; set; }
	}
}
