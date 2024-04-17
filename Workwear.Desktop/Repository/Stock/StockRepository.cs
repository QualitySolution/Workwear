using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Users;
using Workwear.Tools.Features;

namespace Workwear.Repository.Stock
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
				var warehouse = uow.Session.Query<Warehouse>().FirstOrDefault();
				return warehouse;
			}

			UserSettings settings = uow.Session.QueryOver<UserSettings>()
			.Where(x => x.User.Id == idUser).SingleOrDefault<UserSettings>();
			if(settings?.DefaultWarehouse != null) 
				return settings.DefaultWarehouse;

			var warehouses = uow.GetAll<Warehouse>().Take(2).ToList();
			return warehouses.Count == 1 ? warehouses.First() : null; 
		}

		public virtual IList<StockBalanceDTO> StockBalances(
			IUnitOfWork uow, 
			Warehouse warehouse, 
			IEnumerable<Nomenclature> nomenclatures, 
			DateTime onDate, 
			IEnumerable<WarehouseOperation> excludeOperations = null)
		{
			StockBalanceDTO resultAlias = null;
			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			Owner ownerAlias = null;

			var nextDay = onDate.AddDays(1);
			var excludeIds = excludeOperations?.Select(x => x.Id).ToList();
			var nomenclaturesDic = nomenclatures.ToDictionary(n => n.Id, n => n);

			// null == null => null              null <=> null => true
			var expenseQuery = QueryOver.Of(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == warehouseOperationAlias.Nomenclature.Id
				             && (warehouseExpenseOperationAlias.WearSize.Id == warehouseOperationAlias.WearSize.Id
				                 || (warehouseExpenseOperationAlias.WearSize == null && warehouseOperationAlias.WearSize == null))
				             && (warehouseExpenseOperationAlias.Height.Id == warehouseOperationAlias.Height.Id
				                 || (warehouseExpenseOperationAlias.Height == null && warehouseOperationAlias.Height == null))
				             && (warehouseExpenseOperationAlias.Owner.Id == warehouseOperationAlias.Owner.Id
								 || (warehouseExpenseOperationAlias.Owner == null && warehouseOperationAlias.Owner == null))
				             && warehouseExpenseOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < nextDay);

			if(warehouse == null)
				expenseQuery.Where(x => x.ExpenseWarehouse != null);
			else
				expenseQuery.Where(x => x.ExpenseWarehouse == warehouse);

			if(excludeIds != null && excludeIds.Count > 0)
				expenseQuery.WhereNot(x => x.Id.IsIn(excludeIds));

			expenseQuery.Select(Projections
								.Sum(Projections
									.Property(() => warehouseExpenseOperationAlias.Amount)));

			var incomeSubQuery = QueryOver.Of(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == warehouseOperationAlias.Nomenclature.Id 
				             && (warehouseIncomeOperationAlias.WearSize.Id == warehouseOperationAlias.WearSize.Id
				                 || (warehouseIncomeOperationAlias.WearSize == null && warehouseOperationAlias.WearSize == null))
				             && (warehouseIncomeOperationAlias.Height.Id == warehouseOperationAlias.Height.Id
				                 || (warehouseIncomeOperationAlias.Height == null && warehouseOperationAlias.Height == null))
				             && (warehouseIncomeOperationAlias.Owner.Id == warehouseOperationAlias.Owner.Id
								 || (warehouseIncomeOperationAlias.Owner == null && warehouseOperationAlias.Owner == null))
				             && warehouseIncomeOperationAlias.WearPercent == warehouseOperationAlias.WearPercent)
				.Where(e => e.OperationTime < nextDay);

			if(warehouse == null)
				incomeSubQuery.Where(x => x.ReceiptWarehouse != null);
			else
				incomeSubQuery.Where(x => x.ReceiptWarehouse == warehouse);

			if(excludeIds != null && excludeIds.Count > 0)
				incomeSubQuery.WhereNot(x => x.Id.IsIn(excludeIds));

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
			queryStock.Where(Restrictions.Not(Restrictions.Eq(projection, 0)));
			queryStock.Where(() => warehouseOperationAlias.Nomenclature.Id.IsIn(nomenclaturesDic.Keys));

			var result = queryStock
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				 .JoinAlias(() => warehouseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				 .JoinAlias(() => warehouseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Owner, () => ownerAlias, JoinType.LeftOuterJoin)
				.SelectList(list => list
			   .SelectGroup(() => warehouseOperationAlias.Nomenclature.Id).WithAlias(() => resultAlias.NomenclatureId)
			   .SelectGroup(() => warehouseOperationAlias.WearSize).WithAlias(() => resultAlias.WearSize)
			   .SelectGroup(() => warehouseOperationAlias.Height).WithAlias(() => resultAlias.Height)
			   .SelectGroup(() => warehouseOperationAlias.Owner).WithAlias(() => resultAlias.Owner)
			   .SelectGroup(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
			   .Select(projection).WithAlias(() => resultAlias.Amount)
				)
				.TransformUsing(Transformers.AliasToBean<StockBalanceDTO>())
				.List<StockBalanceDTO>();

			//Проставляем номенклатуру.
			result.ToList().ForEach(item => item.Nomenclature = nomenclaturesDic[item.NomenclatureId]);
			return result;
		}
	}

	public class StockBalanceDTO {
		public Nomenclature Nomenclature { get; set; }
		public int NomenclatureId { get; set; }
		public Size WearSize { get; set; }
		public Size Height { get; set; }
		public Owner Owner { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }
		public StockPosition StockPosition => new StockPosition(Nomenclature, WearPercent, WearSize, Height, Owner);
	}
}
