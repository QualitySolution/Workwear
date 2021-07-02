using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using workwear.Domain.Operations;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Measurements;

namespace workwear.Journal.ViewModels.Stock
{
	/// <summary>
	/// Stock balance short summary journal view model. Для отображения количества номенклатуры, без учета размеров. 
	/// </summary>
	public class StockBalanceShortSummaryJournalViewModel : JournalViewModelBase
	{
		public bool ShowSummary;

		public StockBalanceFilterViewModel Filter { get; private set; }

		public StockBalanceShortSummaryJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope) : base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<StockBalanceFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			var dataLoader = new ThreadDataLoader<StockBalanceShortSummaryJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			CreateNodeActions();

			UpdateOnChanges(typeof(WarehouseOperation), typeof(Nomenclature));
			TabName = TabName = "Остатки по складу" + Filter.Warehouse?.Name;

			Filter.PropertyChanged += (sender, e) => TabName = "Остатки по складу " + Filter.Warehouse?.Name;
		}

		protected IQueryOver<WarehouseOperation> ItemsQuery(IUnitOfWork uow)
		{
			StockBalanceShortSummaryJournalNode resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			MeasurementUnits unitsAlias = null;

			// null == null => null              null <=> null => true
			var expensequery = QueryOver.Of<WarehouseOperation>(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == nomenclatureAlias.Id
				&& (warehouseExpenseOperationAlias.Size == warehouseOperationAlias.Size ||
				(warehouseOperationAlias.Size == null && warehouseExpenseOperationAlias.Size == null))
				&& (warehouseExpenseOperationAlias.Growth == warehouseOperationAlias.Growth ||
				(warehouseExpenseOperationAlias.Growth == null && warehouseOperationAlias.Growth == null))
				&& warehouseExpenseOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < DateTime.Now);

			if(Filter.Warehouse == null)
				expensequery.Where(x => x.ExpenseWarehouse != null);
			else
				expensequery.Where(x => x.ExpenseWarehouse == Filter.Warehouse);

			expensequery.Select(Projections.Sum(Projections.Property(() => warehouseExpenseOperationAlias.Amount)));

			var incomeSubQuery = QueryOver.Of<WarehouseOperation>(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == nomenclatureAlias.Id
				&& (warehouseIncomeOperationAlias.Size == warehouseOperationAlias.Size
				|| (warehouseOperationAlias.Size == null && warehouseIncomeOperationAlias.Size == null))
				&& (warehouseIncomeOperationAlias.Growth == warehouseOperationAlias.Growth ||
				(warehouseIncomeOperationAlias.Growth == null && warehouseOperationAlias.Growth == null))
				&& (warehouseIncomeOperationAlias.WearPercent == warehouseOperationAlias.WearPercent))
				.Where(e => e.OperationTime < DateTime.Now);
			if(Filter.Warehouse == null)
				incomeSubQuery.Where(x => x.ReceiptWarehouse != null);
			else
				incomeSubQuery.Where(x => x.ReceiptWarehouse == Filter.Warehouse);

			incomeSubQuery.Select(Projections.Sum(Projections.Property(() => warehouseIncomeOperationAlias.Amount)));

			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.SubQuery(incomeSubQuery),
				Projections.SubQuery(expensequery)
			);

			var queryStock = uow.Session.QueryOver<WarehouseOperation>(() => warehouseOperationAlias);

			if(Filter.ShowNegativeBalance) {
				queryStock.Where(Restrictions.Not(Restrictions.Eq(projection, 0)));
			}
			else {
				queryStock.Where(Restrictions.Gt(projection, 0));
			}

			if(Filter.ItemTypeCategory != null)
				queryStock.Where(() => itemtypesAlias.Category == Filter.ItemTypeCategory);

			//Если у нас выключена способность показывать общие по всем складам остатки. Но не указан склад мы должны показывать пустую таблицу. Это заведомо ложное условие.
			if(ShowSummary == false && Filter.Warehouse == null)
				queryStock.Where(x => x.Id == -1);

			if(Filter.ProtectionTools != null) {
				queryStock.Where(x => x.Nomenclature.IsIn(Filter.ProtectionTools.MatchedNomenclatures.ToArray()));
			}

			return queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemtypesAlias)
				.JoinAlias(() => itemtypesAlias.Units, () => unitsAlias)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => warehouseOperationAlias.Size))

				.SelectList(list => list
			   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
			   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
			   .Select(() => nomenclatureAlias.Sex).WithAlias(() => resultAlias.Sex)
			   .Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)

			   .Select(projection).WithAlias(() => resultAlias.Amount)
							  .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(distinct ?1 SEPARATOR ?2)"),
					   NHibernateUtil.String,
					   Projections.Property(() => warehouseOperationAlias.Size),
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
		public string UnitsName { get; set; }
		public int Amount { get; set; }
		public string Size { get; set; }
		public ClothesSex? Sex { get; set; }

		public string BalanceText => Amount > 0 ? String.Format("{0} {1}", Amount, UnitsName) : String.Format("<span foreground=\"red\">{0}</span> {1}", Amount, UnitsName);


	}
}

