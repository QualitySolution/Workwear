using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
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

		protected IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow)
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

			if(Filter.ItemTypeCategory != null)
				queryStock.Where(() => itemTypesAlias.Category == Filter.ItemTypeCategory);

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
				(selected) => selected.Any(x => {
					StockBalanceJournalNode node = (x as StockBalanceJournalNode);
					return node != null && node.UseBarcode && node.Amount > 0;
				}),
				(selected) => true,
				(selected) => OpenReleaseBarcodesWindow(selected.First() as StockBalanceJournalNode)
			);
			
			NodeActionsList.Add(releaseBarcodesAction);
		}

		void OpenMovements(StockBalanceJournalNode[] nodes)
		{
			foreach(var node in nodes) {
				IPage<StockMovmentsJournalViewModel> journal = NavigationManager
					.OpenViewModel<StockMovmentsJournalViewModel>(this);
				journal.ViewModel.Filter.SetAndRefilterAtOnce(
					filter => filter.Warehouse = Filter.Warehouse, 
					filter => filter.StockPosition = node.GetStockPosition(journal.ViewModel.UoW));
			}
		}

		private void OpenReleaseBarcodesWindow(StockBalanceJournalNode node) 
		{
			NavigationManager.OpenViewModel<StockReleaseBarcodesViewModel, StockBalanceJournalNode>(this, node);
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
