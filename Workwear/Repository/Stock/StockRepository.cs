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
using workwear.Domain.Users;
using workwear.Tools.Features;

namespace workwear.Repository.Stock
{
	public class StockRepository
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Возвращается первый склад из БД, если версия программы с единственным складом.
		/// Возвращает склад по умолчанию, определенный в настройках пользователя, при создании различных документов и прочее.
		/// </summary>
		public virtual Warehouse GetDefaultWarehouse(IUnitOfWork uow, FeaturesService featureService, int idUser)
		{
			if(!featureService.Available(WorkwearFeature.Warehouses)) {
				var warehous = uow.Session.Query<Warehouse>().FirstOrDefault();
				return warehous;
			}

			UserSettings settings = uow.Session.QueryOver<UserSettings>()
			.Where(x => x.User.Id == idUser).SingleOrDefault<UserSettings>();
			if(settings?.DefaultWarehouse != null) 
				return settings.DefaultWarehouse;

			var warehouses = uow.GetAll<Warehouse>().Take(2).ToList();
			return warehouses.Count == 1 ? warehouses.First() : null; 
		}

		public virtual IList<StockBalanceDTO> StockBalances(IUnitOfWork uow, Warehouse warehouse, IList<Nomenclature> nomenclatures, DateTime onTime, IList<WarehouseOperation> excludeOperations = null)
		{
			StockBalanceDTO resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;

			List<int> excludeIds = excludeOperations?.Select(x => x.Id).ToList();

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

			if(excludeIds != null && excludeIds.Count > 0)
				expensequery.WhereNot(x => x.Id.IsIn(excludeIds));

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

			if(excludeIds != null && excludeIds.Count > 0)
				incomeSubQuery.WhereNot(x => x.Id.IsIn(excludeIds));

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