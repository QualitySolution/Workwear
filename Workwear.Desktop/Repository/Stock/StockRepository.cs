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
			WarehouseOperation warehouseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			Owner ownerAlias = null;

			var nextDay = onDate.AddDays(1);
			var excludeIds = excludeOperations?.Select(x => x.Id).ToList();
			var nomenclaturesDic = nomenclatures.ToDictionary(n => n.Id, n => n);
			
			var queryStock = uow.Session.QueryOver(() => warehouseOperationAlias)
				.Where(e => e.OperationTime < nextDay)
				.Where(() => warehouseOperationAlias.Nomenclature.Id.IsIn(nomenclaturesDic.Keys));
			if(excludeIds != null && excludeIds.Count > 0)
				queryStock.WhereNot(x => x.Id.IsIn(excludeIds));
			
			var projectionAll = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "(CASE WHEN ?1 is null THEN ?2 WHEN ?3 is null THEN -(?2) END)"),
				NHibernateUtil.Int32,
				Projections.Property<WarehouseOperation>(x => x.ExpenseWarehouse),
				Projections.Property<WarehouseOperation>(x => x.Amount),
				Projections.Property<WarehouseOperation>(x => x.ReceiptWarehouse)
			);
			var projectionStock = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "CASE WHEN ?1 = ?2 THEN ?3 WHEN ?4 = ?2 THEN -?3 ELSE 0 END"),
				NHibernateUtil.Int32,
				Projections.Property<WarehouseOperation>(x => x.ReceiptWarehouse),
				Projections.Constant(warehouse?.Id),
				Projections.Property<WarehouseOperation>(x => x.Amount),
				Projections.Property<WarehouseOperation>(x => x.ExpenseWarehouse)
			);
			
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
						.Select(Projections.Sum(warehouse == null ? projectionAll : projectionStock)).WithAlias(() => resultAlias.Amount)
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
