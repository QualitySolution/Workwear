using System;
using workwear.Domain.Stock;
using QSOrmProject;
using workwear.Domain;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using workwear.Measurements;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using System.Linq;

namespace workwear
{
	public static class StockRepository
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

			var disjunction= new Disjunction();
			var employeeSize = employee.GetSize (itemType.WearCategory.Value);

			if(employeeSize == null || String.IsNullOrEmpty (employeeSize.Size) || String.IsNullOrEmpty (employeeSize.StandardCode))
			{
				logger.Warn ("В карточке сотрудника не указан размер для спецодежды типа <{0}>.", itemType.Name);
				return null;
			}

			foreach(var pair in SizeHelper.MatchSize (employeeSize))
			{
				disjunction.Add (
					Restrictions.And (
						Restrictions.Eq (Projections.Property<Nomenclature>(n => n.SizeStd), pair.StandardCode),
						Restrictions.Eq (Projections.Property<Nomenclature>(n => n.Size), pair.Size)
					));
			}
			query.Where (disjunction);
			var growthStd = SizeHelper.GetGrowthStandart (itemType.WearCategory.Value, employee.Sex);
			if(growthStd != null)
			{
				query.Where (n => n.WearGrowthStd == SizeHelper.GetSizeStdCode (growthStd) && n.WearGrowth == employee.WearGrowth);
			}

			return query.List ();
		}

		public static IList<StockBalanceDTO> BalanceInStockDetail(IUnitOfWork uow, IList<Nomenclature> nomenclatures)
		{
			StockBalanceDTO resultAlias = null;

			IncomeItem incomeItemAlias = null;
			ExpenseItem expenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;

			var incomes = uow.Session.QueryOver<IncomeItem> (() => incomeItemAlias)
				.WhereRestrictionOn (i => i.Nomenclature).IsInG (nomenclatures);

			var subqueryRemove = QueryOver.Of<ExpenseItem>(() => expenseItemAlias)
				.Where(() => expenseItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<ExpenseItem> (o => o.Amount));

			var subqueryWriteOff = QueryOver.Of<WriteoffItem>(() => writeoffItemAlias)
				.Where(() => writeoffItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<WriteoffItem> (o => o.Amount));

			var incomeList = incomes
				.Where (Restrictions.Gt (
					Projections.SqlFunction(
						new VarArgsSQLFunction("(", "-", ")"),
						NHibernateUtil.Int32,
						Projections.Property (() => incomeItemAlias.Amount),
						Projections.SqlFunction("COALESCE", 
							NHibernateUtil.Int32,
							Projections.SubQuery (subqueryRemove),
							Projections.Constant (0)
						),
						Projections.SqlFunction("COALESCE", 
							NHibernateUtil.Int32,
							Projections.SubQuery (subqueryWriteOff),
							Projections.Constant (0)
						)
					), 0)
				)
				.SelectList (list => list
					.SelectGroup (() => incomeItemAlias.Id).WithAlias (() => resultAlias.IncomeItemId)
					.Select (() => incomeItemAlias.Nomenclature.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => incomeItemAlias.LifePercent).WithAlias (() => resultAlias.Life)
					.Select (() => incomeItemAlias.Amount).WithAlias (() => resultAlias.Income)
					.SelectSubQuery (subqueryRemove).WithAlias (() => resultAlias.Expense)
					.SelectSubQuery (subqueryWriteOff).WithAlias (() => resultAlias.Writeoff)
				)
				.TransformUsing (Transformers.AliasToBean<StockBalanceDTO> ())
				.List<StockBalanceDTO> ();

			//Проставляем номенклатуру.
			incomeList.ToList ().ForEach (item => item.Nomenclature = nomenclatures.First (n => n.Id == item.NomenclatureId));

			return incomeList;
		}

		public static IList<StockBalanceDTO> BalanceInStockSummary(IUnitOfWork uow, IList<Nomenclature> nomenclatures)
		{
			StockBalanceDTO resultAlias = null;

			IncomeItem incomeItemAlias = null;
			ExpenseItem expenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;

			var incomes = uow.Session.QueryOver<IncomeItem> (() => incomeItemAlias)
				.WhereRestrictionOn (i => i.Nomenclature).IsInG (nomenclatures);

			var subqueryRemove = QueryOver.Of<ExpenseItem>(() => expenseItemAlias)
				.Where(() => expenseItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<ExpenseItem> (o => o.Amount));

			var subqueryWriteOff = QueryOver.Of<WriteoffItem>(() => writeoffItemAlias)
				.Where(() => writeoffItemAlias.IncomeOn.Id == incomeItemAlias.Id)
				.Select (Projections.Sum<WriteoffItem> (o => o.Amount));

			var incomeList = incomes
				.SelectList (list => list
					.SelectGroup (() => incomeItemAlias.Nomenclature.Id).WithAlias (() => resultAlias.NomenclatureId)
					//.SelectAvg (() => incomeItemAlias.LifePercent).WithAlias (() => resultAlias.Life)
					.SelectSum (() => incomeItemAlias.Amount).WithAlias (() => resultAlias.Income)
					.SelectSubQuery (subqueryRemove).WithAlias (() => resultAlias.Expense)
					.SelectSubQuery (subqueryWriteOff).WithAlias (() => resultAlias.Writeoff)
				)
				.TransformUsing (Transformers.AliasToBean<StockBalanceDTO> ())
				.List<StockBalanceDTO> ();

			//Проставляем номенклатуру.
			incomeList.ToList ().ForEach (item => item.Nomenclature = nomenclatures.First (n => n.Id == item.NomenclatureId));

			return incomeList;
		}
	}

	public class StockBalanceDTO
	{
		public Nomenclature Nomenclature { get; set;}
		public int IncomeItemId { get; set;}
		public int NomenclatureId { get; set;}

		public int Income { get; set;}
		public int Expense { get; set;}
		public int Writeoff { get; set;}

		public int Amount { get{ return Income - Expense - Writeoff;
			}}
		public decimal Life { get; set;}
	}
}

