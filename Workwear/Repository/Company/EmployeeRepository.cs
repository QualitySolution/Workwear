using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;

namespace workwear.Repository.Company
{
	public class EmployeeRepository
	{
		public IUnitOfWork RepoUow;

		public EmployeeRepository(IUnitOfWork uow = null)
		{
			RepoUow = uow;
		}

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

		public static IList<EmployeeCard> GetActiveEmployeesFromSubdivision(IUnitOfWork uow, int subdivisionId)
		{
			return ActiveEmployeesQuery().GetExecutableQueryOver(uow.Session)
				.Where(x => x.Subdivision.Id == subdivisionId)
				.List();
		}

		public static IList<EmployeeRecivedInfo> ItemsBalance(IUnitOfWork uow, EmployeeCard employee, DateTime onDate)
		{
			EmployeeRecivedInfo resultAlias = null;

			EmployeeIssueOperation employeeIssueOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperationReceivedAlias = null;

			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "SUM(IFNULL(?1, 0) - IFNULL(?2, 0))"),
				NHibernateUtil.Int32,
				Projections.Property<EmployeeIssueOperation>(x => x.Issued),
				Projections.Property<EmployeeIssueOperation>(x => x.Returned)
			);

			IProjection projectionIssueDate = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Date, "MAX(CASE WHEN ?1 > 0 THEN ?2 END)"),
				NHibernateUtil.Date,
				Projections.Property<EmployeeIssueOperation>(x => x.Issued),
				Projections.Property<EmployeeIssueOperation>(x => x.OperationTime)
			);

			return uow.Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.Left.JoinAlias(x => x.IssuedOperation, () => employeeIssueOperationReceivedAlias)
				.Where(x => x.Employee == employee)
				.Where(() => employeeIssueOperationAlias.AutoWriteoffDate == null || employeeIssueOperationAlias.AutoWriteoffDate > onDate)
				.Where(() => employeeIssueOperationReceivedAlias.AutoWriteoffDate == null || employeeIssueOperationReceivedAlias.AutoWriteoffDate > onDate)
				.SelectList(list => list
				   .SelectGroup(() => employeeIssueOperationAlias.ProtectionTools.Id).WithAlias(() => resultAlias.ProtectionToolsId)
				   .SelectGroup(() => employeeIssueOperationAlias.NormItem.Id).WithAlias(() => resultAlias.NormRowId)
				   .Select(projectionIssueDate).WithAlias(() => resultAlias.LastReceive)
				   .Select(projection).WithAlias(() => resultAlias.Amount)
				)
				.TransformUsing(Transformers.AliasToBean<EmployeeRecivedInfo>())
				.List<EmployeeRecivedInfo>();
		}

		#region Norms
		public IList<EmployeeCard> GetEmployeesUseNorm(Norm[] norms, IUnitOfWork uow = null)
		{
			Norm normAlias = null;
			return (uow ?? RepoUow).Session.QueryOver<EmployeeCard>()
				.JoinAlias(x => x.UsedNorms, () => normAlias)
				.Where(x => normAlias.Id.IsIn(norms.GetIds().ToArray()))
				.TransformUsing(Transformers.DistinctRootEntity)
				.List();
		}

		public static IList<EmployeeCard> GetEmployeesDependenceOnNormItem(IUnitOfWork uow, NormItem item)
		{
			EmployeeCardItem employeeItemAlias = null;
			return uow.Session.QueryOver<EmployeeCard>()
				.JoinQueryOver(e => e.WorkwearItems, () => employeeItemAlias)
				.Where(() => employeeItemAlias.ActiveNormItem == item)
				.List();
		}
		#endregion

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

		public virtual EmployeeCard GetEmployeeByCardkey(IUnitOfWork uow, string cardkey)
		{
			return uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.CardKey == cardkey)
				.Take(1)
				.SingleOrDefault();
		}

		public virtual EmployeeCard GetEmployeeByPhone(IUnitOfWork uow, string phone)
		{
			return uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PhoneNumber == phone)
				.Take(1)
				.SingleOrDefault();
		}
	}

	public class EmployeeRecivedInfo
	{
		public int? NormRowId { get; set; }

		public int? ProtectionToolsId { get; set;}

		public DateTime LastReceive { get; set;}

		public int Amount { get; set;}
	}
}

