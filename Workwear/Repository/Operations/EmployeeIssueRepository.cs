using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Models.Operations;

namespace workwear.Repository.Operations
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
		/// Получаем операции выдачи выполненые в определенные даты.
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
		/// Получаем операции числящееся по сотрудникам которых затрагивает опеределенны диапазон дат.
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
			IUnitOfWork uow, EmployeeCard employee, 
			ProtectionTools protectionTools, 
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
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
			NormItem normItem, Action<IQueryOver<EmployeeIssueOperation, 
				EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.NormItem == normItem);
			makeEager?.Invoke(query);
			return query.List();
		}
		public IList<OperationToDocumentReference> GetReferencedDocuments(params int[] operationsIds)
		{
			OperationToDocumentReference docAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			ExpenseItem expenseItemAlias = null;
			IncomeItem incomeItemAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;
			
			var result = RepoUow.Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.JoinEntityAlias(() => expenseItemAlias, () => expenseItemAlias.EmployeeIssueOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => collectiveExpenseItemAlias, () => collectiveExpenseItemAlias.EmployeeIssueOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => incomeItemAlias, () => incomeItemAlias.ReturnFromEmployeeOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => writeoffItemAlias, () => writeoffItemAlias.EmployeeWriteoffOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.Where(x => x.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.Id).WithAlias(() => docAlias.OperationId)
					.Select(() => expenseItemAlias.Id).WithAlias(() => docAlias.ExpenceItemId)
					.Select(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => docAlias.ExpenceId)
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
	}
}
