using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.Repository
{
	public static class EmployeeRepository
	{

		public static QueryOver<EmployeeCard> ActiveEmployeesQuery ()
		{
			return QueryOver.Of<EmployeeCard> ().Where (e => e.DismissDate == null);
		}

		public static IList<EmployeeItemsBalanceDTO> ItemsBalance(IUnitOfWork uow, EmployeeCard employee)
		{
			EmployeeItemsBalanceDTO resultAlias = null;

			Expense reciveDocAlias = null;
			ExpenseItem recivedItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemtypesAlias = null;
			IncomeItem returnedItemAlias = null;
			WriteoffItem writeoffItemAlias = null;

			var incomes = uow.Session.QueryOver<Expense> (() => reciveDocAlias)
				.Where(d => d.Employee == employee)
				.JoinQueryOver (d => d.Items, () => recivedItemAlias)
				.Where (i => i.AutoWriteoffDate == null || i.AutoWriteoffDate > DateTime.Today);

			var subqueryRemove = QueryOver.Of<IncomeItem>(() => returnedItemAlias)
				.Where(() => returnedItemAlias.IssuedOn.Id == recivedItemAlias.Id)
				.Select (Projections.Sum<IncomeItem> (o => o.Amount));

			var subqueryWriteOff = QueryOver.Of<WriteoffItem>(() => writeoffItemAlias)
				.Where(() => writeoffItemAlias.IssuedOn.Id == recivedItemAlias.Id)
				.Select (Projections.Sum<WriteoffItem> (o => o.Amount));

			var incomeList = incomes
				.JoinAlias (() => recivedItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemtypesAlias)
				.Where (Restrictions.Gt (
					Projections.SqlFunction(
						new VarArgsSQLFunction("(", "-", ")"),
						NHibernateUtil.Int32,
						Projections.Property (() => recivedItemAlias.Amount),
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
					.SelectGroup (() => recivedItemAlias.Id).WithAlias (() => resultAlias.ExpenseItemId)
					.Select (() => reciveDocAlias.Date).WithAlias (() => resultAlias.LastReceive)
					.Select (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Type.Id).WithAlias (() => resultAlias.ItemsTypeId)
					.Select (() => recivedItemAlias.Amount).WithAlias (() => resultAlias.Received)
					.SelectSubQuery (subqueryRemove).WithAlias (() => resultAlias.Returned)
					.SelectSubQuery (subqueryWriteOff).WithAlias (() => resultAlias.Writeoff)
				)
				.TransformUsing (Transformers.AliasToBean<EmployeeItemsBalanceDTO> ())
				.List<EmployeeItemsBalanceDTO> ();

			return incomeList;
		}

		public static IList<EmployeeCard> GetEmployeesDependenceOnNormItem(IUnitOfWork uow, NormItem item)
		{
			EmployeeCardItem employeeItemAlias = null;
			return uow.Session.QueryOver<EmployeeCard>()
				.JoinQueryOver(e => e.WorkwearItems, () => employeeItemAlias)
				.Where(() => employeeItemAlias.ActiveNormItem == item)
				.List();
		}
	}

	public class EmployeeItemsBalanceDTO
	{
		public int ExpenseItemId { get; set;}
		public int ItemsTypeId { get; set;}
		public int NomenclatureId { get; set;}

		public DateTime LastReceive { get; set;}

		public int Received { get; set;}
		public int Returned { get; set;}
		public int Writeoff { get; set;}

		public int Amount { get{ return Received - Returned - Writeoff;
			}}
	}
}

