using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;
using Workwear.Models.Operations;

namespace Workwear.Repository.Operations
{
	public class EmployeeIssueRepository
	{
		public IUnitOfWork RepoUow;

		public EmployeeIssueRepository(IUnitOfWork uow = null)
		{
			RepoUow = uow;
		}
		/// <summary>
		/// Получаем все операции выдачи сотруднику отсортированные в порядке убывания.
		/// </summary>
		/// <returns></returns>
		public IList<EmployeeIssueOperation> AllOperationsForEmployee(
			EmployeeCard employee, 
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Desc.List();
		}

		/// <summary>
		/// Получаем операции выдачи выполненные в определенные даты.
		/// </summary>
		public IList<EmployeeIssueOperation> GetOperationsByDates(
			EmployeeCard[] employees, 
			DateTime begin, DateTime end, 
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			var employeeIds = employees.Select(x => x.Id).Distinct().ToArray();
			var query = RepoUow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee.Id.IsIn(employeeIds))
				//Проверяем попадает ли операция в диапазон, обратным стравлением условий.
				//Проверяем 2 даты и начала и конца,
				//так как по сути для наса важны StartOfUse и ExpiryByNorm но они могут быть null.
				.Where(o => (o.OperationTime < end.Date.AddDays(1)) && (o.OperationTime >= begin));
			makeEager?.Invoke(query);
			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		/// <summary>
		/// Получаем операции числящееся по сотрудникам которых затрагивает определенный диапазон дат.
		/// </summary>
		public virtual IList<EmployeeIssueOperation> GetOperationsTouchDates(IUnitOfWork uow, EmployeeCard[] employees, DateTime begin, DateTime end, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null) { 

			var employeeIds = employees.Select(x => x.Id).Distinct().ToArray();

			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee.Id.IsIn(employeeIds))
				//Проверяем попадает ли операция в диапазон, обратным стравлением условий.
				//Проверяем 2 даты и начала и конца,
				//так как по сути для наса важны StartOfUse и ExpiryByNorm но они могут быть null.
				.Where(o => (o.OperationTime <= end || o.StartOfUse <= end) 
				            && (o.ExpiryByNorm >= begin || o.AutoWriteoffDate >= begin));

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public IList<EmployeeIssueOperation> GetOperationsForEmployee(
			EmployeeCard employee, 
			ProtectionTools protectionTools, 
			IUnitOfWork uow = null,
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee)
				.Where(o => o.ProtectionTools == protectionTools);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public IList<EmployeeIssueOperation> GetLastIssueOperationsForEmployee(
			IEnumerable<EmployeeCard> employees, 
			Action<IQueryOver<EmployeeIssueOperation, 
				EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null)
		{
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperation2Alias = null;
			var ids = employees.Select(x => x.Id).Distinct().ToArray();
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.Where(o => o.Employee.IsIn(ids))
				.Where(() => employeeIssueOperationAlias.Issued > 0)
				.JoinEntityAlias(() => employeeIssueOperation2Alias,
					() => employeeIssueOperationAlias.ProtectionTools.Id == employeeIssueOperation2Alias.ProtectionTools.Id
					      && employeeIssueOperationAlias.Employee.Id == employeeIssueOperation2Alias.Employee.Id
					      && employeeIssueOperation2Alias.Issued > 0
					      && employeeIssueOperationAlias.OperationTime < employeeIssueOperation2Alias.OperationTime
					, JoinType.LeftOuterJoin)
				.Where(() => employeeIssueOperation2Alias.Id == null);

			makeEager?.Invoke(query);

			return query.List();
		}

		/// <summary>
		/// Получаем все операции выдачи выданные по указанной строке нормы.
		/// </summary>
		/// <returns></returns>
		public IList<EmployeeIssueOperation> GetOperationsForNormItem(
			NormItem[] normItems, Action<IQueryOver<EmployeeIssueOperation, 
				EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null, DateTime? beginDate = null) {
			var itemIds = normItems.Select(i => i.Id).ToArray();
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.NormItem.Id.IsIn(itemIds));
			if(beginDate.HasValue)
				query.Where(o => o.OperationTime >= beginDate);
			makeEager?.Invoke(query);
			return query.List();
		}
		public IList<OperationToDocumentReference> GetReferencedDocuments(params int[] operationsIds)
		{
			OperationToDocumentReference docAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			Expense expenseAlias = null;
			ExpenseItem expenseItemAlias = null;
			IncomeItem incomeItemAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;
			
			var result = RepoUow.Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.JoinEntityAlias(() => expenseItemAlias, () => expenseItemAlias.EmployeeIssueOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => expenseItemAlias.ExpenseDoc, () => expenseAlias)
				.JoinEntityAlias(() => collectiveExpenseItemAlias, () => collectiveExpenseItemAlias.EmployeeIssueOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => incomeItemAlias, () => incomeItemAlias.ReturnFromEmployeeOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => writeoffItemAlias, () => writeoffItemAlias.EmployeeWriteoffOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.Where(x => x.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.Id).WithAlias(() => docAlias.OperationId)
					.Select(() => expenseItemAlias.Id).WithAlias(() => docAlias.ExpenceItemId)
					.Select(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => docAlias.ExpenceId)
					.Select(() => expenseAlias.Operation).WithAlias(() => docAlias.ExpenseOperation)
					.Select(() => collectiveExpenseItemAlias.Id).WithAlias(() => docAlias.CollectiveExpenseItemId)
					.Select(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => docAlias.CollectiveExpenseId)
					.Select(() => incomeItemAlias.Id).WithAlias(() => docAlias.IncomeItemId)
					.Select(() => incomeItemAlias.Document.Id).WithAlias(() => docAlias.IncomeId)
					.Select(() => writeoffItemAlias.Id).WithAlias(() => docAlias.WriteoffItemId)
					.Select(() => writeoffItemAlias.Document.Id).WithAlias(() => docAlias.WriteoffId)
				)
				.TransformUsing(Transformers.AliasToBean<OperationToDocumentReference>())
				.List<OperationToDocumentReference>();

			foreach(var item in result)
				item.OperationType = OperationType.EmployeeIssue;

			return result;
		}

		public IList<EmployeeIssueOperation> GetAllManualIssue(
			IUnitOfWork uoW, 
			EmployeeCard employee, 
			ProtectionTools protectionTools, 
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null) {
			var query = (uoW ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee)
				.Where(o => o.ProtectionTools == protectionTools)
				.Where(o => o.ManualOperation == true);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}
		
		/// <summary>
		/// Метод подсчитывает количество числящегося за сотрудником на определенную дату.
		/// ВНИМАНИЕ!! Метод пока не учитывает ручные операции скидывающие историю. Это надо будет дорабатывать.
		/// </summary>
		public virtual IList<EmployeeReceivedInfo> ItemsBalance(EmployeeCard employee, DateTime onDate, int[] excludeOperationsIds = null, IUnitOfWork uow = null)
		{
			EmployeeReceivedInfo resultAlias = null;

			EmployeeIssueOperation employeeIssueOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperationReceivedAlias = null;

			if(excludeOperationsIds == null)
				excludeOperationsIds = new int[] { };

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

			return (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.Left.JoinAlias(x => x.IssuedOperation, () => employeeIssueOperationReceivedAlias)
				.Where(x => x.Employee == employee)
				.Where(() => employeeIssueOperationAlias.OperationTime < onDate.AddDays(1))
				.WhereNot(() => employeeIssueOperationAlias.Id.IsIn(excludeOperationsIds))
				.Where(Restrictions.Or(
					Restrictions.Conjunction()
						.Add(Restrictions.Where( () => employeeIssueOperationAlias.Issued > 0))
						.Add(Restrictions.Where( () => employeeIssueOperationAlias.AutoWriteoffDate == null || employeeIssueOperationAlias.AutoWriteoffDate > onDate)),
					Restrictions.Conjunction()
						.Add(Restrictions.Where(() => employeeIssueOperationAlias.Returned > 0))
						.Add(Restrictions.Where(() => employeeIssueOperationReceivedAlias.AutoWriteoffDate == null || employeeIssueOperationReceivedAlias.AutoWriteoffDate > onDate))
					))
				.SelectList(list => list
				   .SelectGroup(() => employeeIssueOperationAlias.ProtectionTools.Id).WithAlias(() => resultAlias.ProtectionToolsId)
				   .SelectGroup(() => employeeIssueOperationAlias.NormItem.Id).WithAlias(() => resultAlias.NormRowId)
				   .Select(projectionIssueDate).WithAlias(() => resultAlias.LastReceive)
				   .Select(projection).WithAlias(() => resultAlias.Amount)
				   .Select(() => employeeIssueOperationAlias.Nomenclature.Id).WithAlias(()=> resultAlias.NomenclatureId)
				)
				.TransformUsing(Transformers.AliasToBean<EmployeeReceivedInfo>())
				.List<EmployeeReceivedInfo>();
		}
	}
	
	public class EmployeeReceivedInfo
	{
		public int? NormRowId { get; set; }

		public int? ProtectionToolsId { get; set;}
		
		public int? NomenclatureId { get; set; }

		public DateTime LastReceive { get; set;}

		public int Amount { get; set;}
	}
}
