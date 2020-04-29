using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.Repository.Company
{
	public static class EmployeeRepository
	{

		public static QueryOver<EmployeeCard> ActiveEmployeesQuery ()
		{
			return QueryOver.Of<EmployeeCard> ().Where (e => e.DismissDate == null);
		}

		public static IList<EmployeeCard> GetActiveEmployeesFromSubdivision(IUnitOfWork uow, Subdivision subdivision)
		{
			return ActiveEmployeesQuery().GetExecutableQueryOver(uow.Session)
				.Where(x => x.Subdivision == subdivision)
				.List();
		}

		public static IList<EmployeeRecivedInfo> ItemsBalance(IUnitOfWork uow, EmployeeCard employee)
		{
			EmployeeRecivedInfo resultAlias = null;

			EmployeeIssueOperation employeeIssueOperationAlias = null;
			Nomenclature nomenclatureAlias = null;

			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "SUM(IFNULL(?1, 0) - IFNULL(?2, 0))"),
				NHibernateUtil.Int32,
				Projections.Property<EmployeeIssueOperation>(x => x.Issued),
				Projections.Property<EmployeeIssueOperation>(x => x.Returned)
			);

			var incomeList = uow.Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.Where(x => x.Employee == employee)
				.JoinAlias (() => employeeIssueOperationAlias.Nomenclature, () => nomenclatureAlias)
				.SelectList (list => list
					.SelectGroup (() => nomenclatureAlias.Type.Id).WithAlias (() => resultAlias.ItemsTypeId)
					.SelectMax (() => employeeIssueOperationAlias.OperationTime).WithAlias (() => resultAlias.LastReceive)
					.Select(projection).WithAlias (() => resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean<EmployeeRecivedInfo> ())
				.List<EmployeeRecivedInfo> ();

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

		public static IList<EmployeeCardItem> GetItems(IUnitOfWork uow, EmployeeCard[] employees, DateTime begin, DateTime end)
		{
			return uow.Session.QueryOver<EmployeeCardItem>()
				.Where(x => x.EmployeeCard.IsIn(employees))
				.Where(x => x.NextIssue >= begin && x.NextIssue <= end)
				.List();
		}

		public static EmployeeCard GetEmployeeByPersonalNumber(IUnitOfWork uow, string personalNumber)
		{
			return uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber == personalNumber)
				.SingleOrDefault();
		}
	}

	public class EmployeeRecivedInfo
	{
		public int ItemsTypeId { get; set;}

		public DateTime LastReceive { get; set;}

		public int Amount { get; set;}
	}
}

