using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;
using workwear.Domain.Stock;

namespace workwear.Repository.Stock
{
	public class StockRepository
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Возвращает склад по умолчанию при создании различных документов и прочее. Сейчас возвращается склад если он единственный,
		/// чтобы его не запнять, в будущем скдал по умолчанию можно будет настроит у пользователя. А так же этот метод дожне будет создавать
		/// новый склад в версиях программы без поддежки складов, чтобы не оказалось что склада нет вообще.
		/// </summary>
		public virtual Warehouse GetDefaultWarehouse(IUnitOfWork uow)
		{
			var warehouses = uow.GetAll<Warehouse>().Take(2).ToList();
			return warehouses.Count == 1 ? warehouses.First() : null; 
		}

		public virtual IList<StockBalanceDTO> StockBalances(IUnitOfWork uow, Warehouse warehouse, IList<Nomenclature> nomenclatures, DateTime onTime)
		{
			StockBalanceDTO resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;

			// null == null => null              null <=> null => true
			var expensequery = QueryOver.Of<WarehouseOperation>(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == nomenclatureAlias.Id
				&& (warehouseExpenseOperationAlias.Size == warehouseOperationAlias.Size ||
				(warehouseOperationAlias.Size == null && warehouseExpenseOperationAlias.Size == null))
				&& (warehouseExpenseOperationAlias.Growth == warehouseOperationAlias.Growth ||
				(warehouseExpenseOperationAlias.Growth == null && warehouseOperationAlias.Growth == null))
				&& warehouseExpenseOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime <= onTime);

			if(warehouse == null)
				expensequery.Where(x => x.ExpenseWarehouse != null);
			else
				expensequery.Where(x => x.ExpenseWarehouse == warehouse);

			expensequery.Select(Projections.Sum(Projections.Property(() => warehouseExpenseOperationAlias.Amount)));

			var incomeSubQuery = QueryOver.Of<WarehouseOperation>(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == nomenclatureAlias.Id
				&& (warehouseIncomeOperationAlias.Size == warehouseOperationAlias.Size
				|| (warehouseOperationAlias.Size == null && warehouseIncomeOperationAlias.Size == null))
				&& (warehouseIncomeOperationAlias.Growth == warehouseOperationAlias.Growth ||
				(warehouseIncomeOperationAlias.Growth == null && warehouseOperationAlias.Growth == null))
				&& (warehouseIncomeOperationAlias.WearPercent == warehouseOperationAlias.WearPercent))

				.Where(e => e.OperationTime < DateTime.Now);


			if(warehouse == null)
				incomeSubQuery.Where(x => x.ReceiptWarehouse != null);
			else
				incomeSubQuery.Where(x => x.ReceiptWarehouse == warehouse);

			incomeSubQuery.Select(Projections.Sum(Projections.Property(() => warehouseIncomeOperationAlias.Amount)));

			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.SubQuery(incomeSubQuery),
				Projections.SubQuery(expensequery)
			);

			var queryStock = uow.Session.QueryOver<WarehouseOperation>(() => warehouseOperationAlias);
			queryStock.Where(Restrictions.Not(Restrictions.Eq(projection, 0)));
			queryStock.Where(() => warehouseOperationAlias.Nomenclature.IsIn(nomenclatures.ToArray()));

			var result = queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.SelectList(list => list
			   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
			   .SelectGroup(() => warehouseOperationAlias.Size).WithAlias(() => resultAlias.Size)
			   .SelectGroup(() => warehouseOperationAlias.Growth).WithAlias(() => resultAlias.Growth)
			   .SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
			   .Select(projection).WithAlias(() => resultAlias.Amount)
				)
				.TransformUsing(Transformers.AliasToBean<StockBalanceDTO>())
				.List<StockBalanceDTO>();

			//Проставляем номенклатуру.
			result.ToList().ForEach(item => item.Nomenclature = nomenclatures.First(n => n.Id == item.NomenclatureId));
			return result;
		}
	}

	public class StockBalanceDTO
	{
		public Nomenclature Nomenclature { get; set; }
		public int NomenclatureId { get; set; }

		public string Size { get; set; }
		public string Growth { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }

		public StockPosition StockPosition => new StockPosition(Nomenclature, Size, Growth, WearPercent);
	}
}