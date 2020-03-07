using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Measurements;

namespace workwear
{
	public class StockRepository
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static IList<Nomenclature> MatchNomenclaturesBySize(IUnitOfWork uow, ItemsType itemType, EmployeeCard employee)
		{
			if(itemType.WearCategory == null)
			{
				logger.Warn ("Вызван подбор номерклатур по размеру, для типа <{0}>, но в нем не указан вид спецодежды.", itemType.Name);
				return null;
			}

			var query = uow.Session.QueryOver<Nomenclature> ()
				.Where (n => n.Type == itemType);

			if (SizeHelper.HasСlothesSizeStd(itemType.WearCategory.Value))
			{
				var disjunction = new Disjunction();
				var employeeSize = employee.GetSize(itemType.WearCategory.Value);

				if (employeeSize == null || String.IsNullOrEmpty(employeeSize.Size) || String.IsNullOrEmpty(employeeSize.StandardCode))
				{
					logger.Warn("В карточке сотрудника не указан размер для спецодежды типа <{0}>.", itemType.Name);
					return null;
				}

				foreach (var pair in SizeHelper.MatchSize (employeeSize, SizeUsePlace.Сlothes))
				{
					disjunction.Add(
						Restrictions.And(
							Restrictions.Eq(Projections.Property<Nomenclature>(n => n.SizeStd), pair.StandardCode),
							Restrictions.Eq(Projections.Property<Nomenclature>(n => n.Size), pair.Size)
						));
				}
				query.Where(disjunction);
				if (SizeHelper.HasGrowthStandart(itemType.WearCategory.Value))
				{
					var growDisjunction = new Disjunction();
					var growStds = SizeHelper.GetGrowthStandart(itemType.WearCategory.Value, employee.Sex, SizeUsePlace.Сlothes);
					foreach (var pair in SizeHelper.MatchGrow (growStds, employee.WearGrowth, SizeUsePlace.Сlothes))
					{
						growDisjunction.Add(
							Restrictions.And(
								Restrictions.Eq(Projections.Property<Nomenclature>(n => n.WearGrowthStd), pair.StandardCode),
								Restrictions.Eq(Projections.Property<Nomenclature>(n => n.WearGrowth), pair.Size)
							));
					}

					query.Where(growDisjunction);
				}
			}

			return query.List ();
		}

		public virtual IList<StockBalanceDTO> StockBalances(IUnitOfWork uow, Warehouse warehouse, IList<Nomenclature> nomenclatures)
		{
			StockBalanceDTO resultAlias = null;

			WarehouseOperation warehouseExpenseOperationAlias = null;
			WarehouseOperation warehouseIncomeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			Nomenclature nomenclatureAlias = null;

			var expensequery = QueryOver.Of<WarehouseOperation>(() => warehouseExpenseOperationAlias)
				.Where(() => warehouseExpenseOperationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(e => e.OperationTime < DateTime.Now);
			if(warehouse == null)
				expensequery.Where(x => x.ExpenseWarehouse != null);
			else
				expensequery.Where(x => x.ExpenseWarehouse == warehouse);

			expensequery.Select(Projections.Sum(Projections.Property(() => warehouseExpenseOperationAlias.Amount)));

			var incomeSubQuery = QueryOver.Of<WarehouseOperation>(() => warehouseIncomeOperationAlias)
				.Where(() => warehouseIncomeOperationAlias.Nomenclature.Id == nomenclatureAlias.Id)
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
		public Nomenclature Nomenclature { get; set;}
		public int NomenclatureId { get; set;}

		public string Size { get; set; }
		public string Growth { get; set; }
		public decimal WearPercent { get; set; }
		public int Amount { get; set; }

		public StockPosition StockPosition => new StockPosition(Nomenclature, Size, Growth, WearPercent);
	}
}