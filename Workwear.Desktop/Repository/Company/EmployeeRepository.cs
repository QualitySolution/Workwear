using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;
using NHibernate.Engine;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Models.Import;

namespace Workwear.Repository.Company
{
	public class EmployeeRepository
	{
		public IUnitOfWork RepoUow;

		public EmployeeRepository(IUnitOfWork uow = null)
		{
			RepoUow = uow;
		}

		public IQueryOver<EmployeeCard, EmployeeCard> ActiveEmployeesQuery (IUnitOfWork uow = null)
		{
			return (uow ?? RepoUow).Session.QueryOver<EmployeeCard>().Where (e => e.DismissDate == null);
		}

		public IList<EmployeeCard> GetActiveEmployeesFromSubdivision(IUnitOfWork uow, Subdivision subdivision)
		{
			return ActiveEmployeesQuery(uow)
				.Where(x => x.Subdivision == subdivision)
				.List();
		}

		public IList<EmployeeCard> GetActiveEmployeesFromSubdivision(IUnitOfWork uow, int subdivisionId)
		{
			return ActiveEmployeesQuery(uow)
				.Where(x => x.Subdivision.Id == subdivisionId)
				.List();
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

		#region GetEmployes

		public IQueryOver<EmployeeCard,EmployeeCard> GetEmployeesByPersonalNumbers(string[] personalNumbers) {
			return RepoUow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber.IsIn(personalNumbers));
		}

		public IQueryOver<EmployeeCard,EmployeeCard> GetEmployeesByFIOs(IEnumerable<FIO> fios) { 
			//Данный диалект используется в тестах.
			if(((ISessionFactoryImplementor) RepoUow.Session.SessionFactory).Dialect is SQLiteDialect) {
				//По сути отдельная реализация метода для тестов на SQLite, так как во первых:
				//SQLite не поддерживает функцию CONCAT_WS
				//Во вторых: на SQLite метод upper работает только для ASCII символов, что не позволяет его использовать для русского языка.
				// https://www.sqlitetutorial.net/sqlite-functions/sqlite-upper/
				// для SQLite мы используем TitleCase так как при сохранении в базу наша программа приводит имена к формату первой заглавной буквы.
				var searchValues = fios
					.Where(fio => !String.IsNullOrEmpty(fio.LastName) && !String.IsNullOrEmpty(fio.FirstName))
					.Select(fio => fio.LastName.StringToTitleCase() + "|" + fio.FirstName.StringToTitleCase())
					.Select(fio => fio.Replace('ё', 'е').Replace('Ё', 'Е'))
					.Distinct().ToArray();
				return RepoUow.Session.QueryOver<EmployeeCard>()
					.Where(Restrictions.In(
							Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.String, "( ?1 || '|' || ?2)"),
									NHibernateUtil.String,
									replaceYoProjection(Projections.Property<EmployeeCard>(x => x.LastName)),
									replaceYoProjection(Projections.Property<EmployeeCard>(x => x.FirstName))
									),
						searchValues));
		    }
			else {
				var searchValues = fios
					.Where(fio => !String.IsNullOrEmpty(fio.LastName) && !String.IsNullOrEmpty(fio.FirstName))
					.Select(fio => (fio.LastName + "|" + fio.FirstName).ToUpper().Replace('Ё', 'Е'))
					.Distinct().ToArray();
				return RepoUow.Session.QueryOver<EmployeeCard>()
					.Where(Restrictions.In(
							Projections.SqlFunction(
								"upper", NHibernateUtil.String,
								Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
									NHibernateUtil.String,
									Projections.Constant(""),
									replaceYoProjection(Projections.Property<EmployeeCard>(x => x.LastName)),
									Projections.Constant("|"),
									replaceYoProjection(Projections.Property<EmployeeCard>(x => x.FirstName))
								)),
						searchValues));
			}
		}
		
		static IProjection replaceYoProjection(IProjection property) {
			return Projections.SqlFunction("replace", NHibernateUtil.String, 
					Projections.SqlFunction("replace", NHibernateUtil.String, property, Projections.Constant("ё"), Projections.Constant("е"))
				, Projections.Constant("Ё"), Projections.Constant("Е"));
		}
		#endregion
	}
}

