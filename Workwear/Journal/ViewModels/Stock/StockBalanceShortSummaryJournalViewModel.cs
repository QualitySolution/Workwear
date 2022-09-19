using System;
using System.Linq;
using Autofac;
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
using Workwear.Measurements;

namespace workwear.Journal.ViewModels.Stock
{
	/// <summary>
	/// Stock balance short summary journal view model. Для отображения количества номенклатуры, без учета размеров. 
	/// </summary>
	public class StockBalanceShortSummaryJournalViewModel : JournalViewModelBase
	{
		public bool ShowSummary;

		public StockBalanceFilterViewModel Filter { get; private set; }

		public StockBalanceShortSummaryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			ILifetimeScope autofacScope) : base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<StockBalanceFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockBalanceShortSummaryJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation), typeof(Nomenclature));
			TabName = "Остатки по складу" + Filter.Warehouse?.Name;

			Filter.PropertyChanged += (sender, e) => 
				TabName = "Остатки по складу " + Filter.Warehouse?.Name;
		}

		protected IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow)
		{
			StockBalanceShortSummaryJournalNode resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;

			// null == null => null              null <=> null => true
			var expenseQuery = QueryOver.Of(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == nomenclatureAlias.Id
				             && (warehouseExpenseOperationAlias.WearSize.Id == sizeAlias.Id ||
				                 sizeAlias == null && warehouseExpenseOperationAlias.WearSize == null)
				             && (warehouseExpenseOperationAlias.Height.Id == heightAlias.Id ||
				                 warehouseExpenseOperationAlias.Height == null && heightAlias == null)
				             && warehouseExpenseOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < DateTime.Now);

			if(Filter.Warehouse == null)
				expenseQuery.Where(x => x.ExpenseWarehouse != null);
			else
				expenseQuery.Where(x => x.ExpenseWarehouse == Filter.Warehouse);

			expenseQuery.Select(Projections
								.Sum(Projections
									.Property(() => warehouseExpenseOperationAlias.Amount)));

			var incomeSubQuery = QueryOver.Of(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == nomenclatureAlias.Id
				             && (warehouseIncomeOperationAlias.WearSize.Id == sizeAlias.Id
				                 || sizeAlias == null && warehouseIncomeOperationAlias.WearSize == null)
				             && (warehouseIncomeOperationAlias.Height.Id == heightAlias.Id ||
				                 warehouseIncomeOperationAlias.Height == null && heightAlias == null)
				             && warehouseIncomeOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < DateTime.Now);
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
			//Но не указан склад мы должны показывать пустую таблицу. Это заведомо ложное условие.
			if(ShowSummary == false && Filter.Warehouse == null)
				queryStock.Where(x => x.Id == -1);

			if(Filter.ProtectionTools != null) {
				queryStock.Where(x => x.Nomenclature.IsIn(Filter.ProtectionTools.MatchedNomenclatures.ToArray()));
			}

			return queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias(() => warehouseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Number,
					() => nomenclatureAlias.Name,
					() => sizeAlias.Name,
					() => heightAlias.Name))

				.SelectList(list => list
			   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
			   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
			   .Select(() => nomenclatureAlias.Number).WithAlias(() => resultAlias.NomenclatureNumber)
			   .Select(() => nomenclatureAlias.Sex).WithAlias(() => resultAlias.Sex)
			   .Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)

			   .Select(projection).WithAlias(() => resultAlias.Amount)
							  .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(distinct ?1 SEPARATOR ?2)"),
					   NHibernateUtil.String,
					   Projections.Property(() => sizeAlias.Name),
					   Projections.Constant(", "))
				   ).WithAlias(() => resultAlias.Size)
				)
				.OrderBy(() => nomenclatureAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<StockBalanceShortSummaryJournalNode>());
		}
	}
	public class StockBalanceShortSummaryJournalNode
	{
		public int Id { get; set; }
		public string NomenclatureName { get; set; }
		public string NomenclatureNumber { get; set; }
		public string UnitsName { get; set; }
		public int Amount { get; set; }
		public string Size { get; set; }
		public ClothesSex? Sex { get; set; }
		public string BalanceText => 
			Amount > 0 ? $"{Amount} {UnitsName}" : $"<span foreground=\"red\">{Amount}</span> {UnitsName}";
	}
}

