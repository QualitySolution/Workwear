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
		public IList<EmployeeIssueOperation> AllOperationsForEmployee(EmployeeCard employee, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Desc.List();
		}

		[Obsolete("Используйте нестатический класс для этого запроса.")]
		public static Func<IUnitOfWork, EmployeeCard, DateTime, DateTime, IList<EmployeeIssueOperation>> GetOperationsTouchDatesTestGap;
		[Obsolete("Используйте нестатический класс для этого запроса.")]
		public static IList<EmployeeIssueOperation> GetOperationsTouchDates(IUnitOfWork uow, EmployeeCard employee, DateTime begin, DateTime end, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			if(GetOperationsTouchDatesTestGap != null)
				return GetOperationsTouchDatesTestGap(uow, employee, begin, end);

			var instance = new EmployeeIssueRepository();

			return instance.GetOperationsTouchDates(uow, new[] { employee }, begin, end, makeEager);
		}

		/// <summary>
		/// Получаем операции выдачи выполненые в определенные даты.
		/// </summary>
		public IList<EmployeeIssueOperation> GetOperationsByDates(EmployeeCard[] employees, DateTime begin, DateTime end, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			var employeeIds = employees.Select(x => x.Id).Distinct().ToArray();

			var query = RepoUow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee.Id.IsIn(employeeIds))
				//Проверяем попадает ли операция в диапазон, обратным стравлением условий. Проверяем 2 даты и начала и конца, так как по сути для наса важны StartOfUse и ExpiryByNorm но они могут быть null.
				.Where(o => (o.OperationTime < end.Date.AddDays(1)) && (o.OperationTime >= begin));

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		/// <summary>
		/// Получаем операции числящееся по сотрудникам которых затрагивает опеределенны диапазон дат.
		/// </summary>
		public IList<EmployeeIssueOperation> GetOperationsTouchDates(IUnitOfWork uow, EmployeeCard[] employees, DateTime begin, DateTime end, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null) { 

			var employeeIds = employees.Select(x => x.Id).Distinct().ToArray();

			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee.Id.IsIn(employeeIds))
				//Проверяем попадает ли операция в диапазон, обратным стравлением условий. Проверяем 2 даты и начала и конца, так как по сути для наса важны StartOfUse и ExpiryByNorm но они могут быть null.
				.Where(o => (o.OperationTime <= end || o.StartOfUse <= end) && (o.ExpiryByNorm >= begin || o.AutoWriteoffDate >= begin));

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public IList<EmployeeIssueOperation> GetOperationsForEmployee(IUnitOfWork uow, EmployeeCard employee, ProtectionTools protectionTools, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee)
				.Where(o => o.ProtectionTools == protectionTools);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public IList<EmployeeIssueReference> GetReferencedDocuments(params int[] operationsIds)
		{
			EmployeeIssueReference docAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			ExpenseItem expenseItemAlias = null;
			IncomeItem incomeItemAlias = null;
			MassExpenseOperation massExpenseOperationAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			WriteoffItem writeoffItemAlias = null;
			
			return RepoUow.Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
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
				.TransformUsing(Transformers.AliasToBean<EmployeeIssueReference>())
				.List<EmployeeIssueReference>();
		}
	}

	public class EmployeeIssueReference
	{
		public int OperationId;
		public int? ExpenceId;
		public int? ExpenceItemId;
		public int? IncomeId;
		public int? IncomeItemId;
		public int? CollectiveExpenseId;
		public int? CollectiveExpenseItemId;
		public int? WriteoffId;
		public int? WriteoffItemId;
		public int? MassExpenseId;
		public int? MassOperationItemItemId;

		
		
		public StokDocumentType? DocumentType {
			get {
				if (ExpenceId.HasValue)
					return StokDocumentType.ExpenseEmployeeDoc;
				if (CollectiveExpenseId.HasValue)
					return StokDocumentType.CollectiveExpense;
				if (IncomeId.HasValue)
					return StokDocumentType.IncomeDoc;
				if (WriteoffId.HasValue)
					return StokDocumentType.WriteoffDoc;

				return null;
			}
		}
		//Внимание здесь последовательность получения ID желательно сохранять такую же как у типа документа.
		//Так как в случае ошибочной связи операции с двумя документами возьмется первый надейнных с обоих случаях, не тип из одного а id от другого.
		public int? DocumentId => ExpenceId ?? CollectiveExpenseId ?? IncomeId ?? WriteoffId;
	}
}
