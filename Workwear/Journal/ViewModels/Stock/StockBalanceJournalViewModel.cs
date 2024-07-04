using System.Linq;
using Autofac;
using Gamma.Utilities;
using Gamma.Widgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock.Widgets;

namespace workwear.Journal.ViewModels.Stock
{
	/// <summary>
	/// Stock balance journal view model. Для подробного отображения баланса склада
	/// </summary>
	public class StockBalanceJournalViewModel : JournalViewModelBase
	{
		public bool ShowSummary;
		public readonly FeaturesService FeaturesService;

		public StockBalanceFilterViewModel Filter { get; private set; }

		public StockBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			FeaturesService featuresService) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			JournalFilter = Filter = autofacScope.Resolve<StockBalanceFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockBalanceJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation), typeof(Nomenclature));
			TabName = "Остатки по складу " + 
			          (featuresService.Available(WorkwearFeature.Warehouses) ? Filter.Warehouse?.Name : "");

			Filter.PropertyChanged += (sender, e) => 
				TabName = "Остатки по складу " + 
				          (featuresService.Available(WorkwearFeature.Warehouses) ? Filter.Warehouse?.Name : "");
			this.FeaturesService = featuresService;
		}
		
		private IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow) 
		{
			if (Filter.ShowWithBarcodes) 
			{
				return BarcodesStockBalanceQuery(uow);
			}
			
			return StockBalance(uow);
		}
		
		private IQueryOver<WarehouseOperation> StockBalance(IUnitOfWork uow) 
		{
			StockBalanceJournalNode resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			Owner ownerAlias = null;
			
			// null == null => null              null <=> null => true
			var expenseQuery = QueryOver.Of(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == warehouseOperationAlias.Nomenclature.Id
				             && (warehouseExpenseOperationAlias.WearSize.Id == warehouseOperationAlias.WearSize.Id
				                 || warehouseExpenseOperationAlias.WearSize == null && warehouseOperationAlias.WearSize == null)
				             && (warehouseExpenseOperationAlias.Height.Id == warehouseOperationAlias.Height.Id
				                 || warehouseExpenseOperationAlias.Height == null && warehouseOperationAlias.Height == null)
				             && (warehouseExpenseOperationAlias.Owner.Id == warehouseOperationAlias.Owner.Id
				                 || warehouseExpenseOperationAlias.Owner == null && warehouseOperationAlias.Owner == null)
				             && warehouseExpenseOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < Filter.Date.AddDays(1));

			if(Filter.Warehouse == null)
				expenseQuery.Where(x => x.ExpenseWarehouse != null);
			else
				expenseQuery.Where(x => x.ExpenseWarehouse == Filter.Warehouse);

			expenseQuery.Select(Projections
								.Sum(Projections
									.Property(() => warehouseExpenseOperationAlias.Amount)));
			
			if(Filter.SelectOwner != null)
				switch(Filter.SelectOwner) {
					case (SpecialComboState.All): break; //все
					case (SpecialComboState.Not): expenseQuery.Where(x => x.Owner == null); break; //без собственника 
					default: expenseQuery.Where(x => x.Owner.Id == DomainHelper.GetId(Filter.SelectOwner)); break;
				}

			var incomeSubQuery = QueryOver.Of(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == warehouseOperationAlias.Nomenclature.Id 
				             && (warehouseIncomeOperationAlias.WearSize.Id == warehouseOperationAlias.WearSize.Id
				                 || warehouseIncomeOperationAlias.WearSize == null && warehouseOperationAlias.WearSize == null)
				             && (warehouseIncomeOperationAlias.Height.Id == warehouseOperationAlias.Height.Id
				                 || warehouseIncomeOperationAlias.Height == null && warehouseOperationAlias.Height == null)
				             && (warehouseIncomeOperationAlias.Owner.Id == warehouseOperationAlias.Owner.Id
				                 || warehouseIncomeOperationAlias.Owner == null && warehouseOperationAlias.Owner == null)
				             && warehouseIncomeOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < Filter.Date.AddDays(1));
			if(Filter.Warehouse == null)
				incomeSubQuery.Where(x => x.ReceiptWarehouse != null);
			else
				incomeSubQuery.Where(x => x.ReceiptWarehouse == Filter.Warehouse);

			incomeSubQuery.Select(Projections
								.Sum(Projections
									.Property(() => warehouseIncomeOperationAlias.Amount)));

			if(Filter.SelectOwner != null)
				switch(Filter.SelectOwner) {
					case (SpecialComboState.All): break;  //все
					case (SpecialComboState.Not): incomeSubQuery.Where(x => x.Owner == null); break; //без собственника
					default: incomeSubQuery.Where(x => x.Owner.Id == DomainHelper.GetId(Filter.SelectOwner)); break; 
				}
			
			var projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.SubQuery(incomeSubQuery),
				Projections.SubQuery(expenseQuery)
			);

			var queryStock = uow.Session.QueryOver(() => warehouseOperationAlias);

			queryStock.Where(Filter.ShowNegativeBalance
				? Restrictions.Not(Restrictions.Eq(projection, 0))
				: Restrictions.Gt(projection, 0));

			if(Filter.ItemsType != null)
				queryStock.Where(() => itemTypesAlias.Id == Filter.ItemsType.Id);

			//Если у нас выключена способность показывать общие по всем складам остатки.
			//Но не указан склад мы должны показывать пустую таблицу.
			//Это заведомо ложное условие.
			if(ShowSummary == false && Filter.Warehouse == null)
				queryStock.Where(x => x.Id == -1);

			if (Filter.ProtectionTools != null) {
				queryStock.Where(x 
					=> x.Nomenclature.IsIn(Filter.ProtectionTools.Nomenclatures.ToArray()));
			}

			return queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias(() => warehouseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Owner, () => ownerAlias, JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.Number,
					() => nomenclatureAlias.Name,
					() => sizeAlias.Name,
					() => heightAlias.Name))

				.SelectList(list => list
			   .SelectGroup(() => warehouseOperationAlias.Nomenclature.Id).WithAlias(() => resultAlias.Id)
			   .Select(() => warehouseOperationAlias.ReceiptWarehouse.Id).WithAlias(() => resultAlias.WarehouseId)
			   .Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomeclatureId)
			   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
			   .Select(() => nomenclatureAlias.Number).WithAlias(() => resultAlias.NomenclatureNumber)
			   .Select(() => nomenclatureAlias.Sex).WithAlias(() => resultAlias.Sex)
			   .Select(() => nomenclatureAlias.UseBarcode).WithAlias(() => resultAlias.UseBarcode)
			   .Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
			   .Select(() => sizeAlias.Name).WithAlias(() => resultAlias.SizeName)
			   .Select(() => heightAlias.Name).WithAlias(() => resultAlias.HeightName)
			   .Select(() => ownerAlias.Name).WithAlias(() => resultAlias.OwnerName)
			   .Select(() => nomenclatureAlias.SaleCost).WithAlias( () => resultAlias.SaleCost)
			   .SelectGroup(() => sizeAlias.Id).WithAlias(() => resultAlias.SizeId)
			   .SelectGroup(() => heightAlias.Id).WithAlias(() => resultAlias.HeightId)
			   .SelectGroup(() => ownerAlias.Id).WithAlias(() => resultAlias.OwnerId)
			   .SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
			   .Select(projection).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => nomenclatureAlias.Name).Asc
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
					Projections.Property(() => heightAlias.Name))).Asc
				.TransformUsing(Transformers.AliasToBean<StockBalanceJournalNode>());
		}
		
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
		
		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			var updateStatusAction = new JournalAction("Показать движения",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => OpenMovements(selected.Cast<StockBalanceJournalNode>().ToArray())
					);
			NodeActionsList.Add(updateStatusAction);
			
			JournalAction releaseBarcodesAction = new JournalAction("Создать штрихкоды",
				(selected) => selected.Any(x => 
				{
					StockBalanceJournalNode node = (x as StockBalanceJournalNode);
					return node != null && node.UseBarcode && node.Amount > 0;
				}),
				(selected) => !Filter.ShowWithBarcodes,
				(selected) => OpenReleaseBarcodesWindow(selected.First() as StockBalanceJournalNode)
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

		private void OpenReleaseBarcodesWindow(StockBalanceJournalNode node) 
		{
			NavigationManager.OpenViewModel<StockReleaseBarcodesViewModel, StockBalanceJournalNode, Warehouse>(this, node, Filter.Warehouse);
		}
	}

	public class StockBalanceJournalNode
	{
		public int Id { get; set; }
		public int WarehouseId { get; set; }
		public int NomeclatureId { get; set; }
		public string NomenclatureName { get; set; }
		public string NomenclatureNumber { get; set; }
		public ClothesSex Sex { get; set; }
		public string SexText => Sex.GetEnumShortTitle();
		public string UnitsName { get; set; }
		public string SizeName { get; set; }
		public int SizeId { get; set; }
		public string HeightName { get; set; }
		public int HeightId { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }
		public int OwnerId { get; set; }
		public string OwnerName { get; set; }
		public bool UseBarcode { get; set; }
		public decimal SaleCost { get; set; }
		public string SaleCostText => SaleCost > 0 ? CurrencyWorks.GetShortCurrencyString (SaleCost) : string.Empty;
		public string BalanceText => Amount > 0 ? 
			$"{Amount} {UnitsName}" : $"<span foreground=\"red\">{Amount}</span> {UnitsName}";
		public string WearPercentText => WearPercent.ToString("P0");

		public StockPosition GetStockPosition(IUnitOfWork uow) => new StockPosition(
			uow.GetById<Nomenclature>(Id), 
			WearPercent, 
			uow.GetById<Size>(SizeId), 
			uow.GetById<Size>(HeightId),
			uow.GetById<Owner>(OwnerId));
	}
}
