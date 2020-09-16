using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using System.Linq;

namespace workwear.Repository.Operations
{
	public class EmployeeIssueRepository
	{
		public static IList<EmployeeIssueOperation> AllOperationsForEmployee(IUnitOfWork uow, EmployeeCard employee, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager)
		{
			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
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

		public IList<EmployeeIssueOperation> GetOperationsTouchDates(IUnitOfWork uow, EmployeeCard[] employees, DateTime begin, DateTime end, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null) { 

			var employeeIds = employees.Select(x => x.Id).Distinct().ToArray();

			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee.Id.IsIn(employeeIds))
				//Проверяем попадает ли операция в диапазон, обратным стравлением условий. Проверяем 2 даты и начала и конца, так как по сути для наса важны StartOfUse и ExpiryByNorm но они могут быть null.
				.Where(o => (o.OperationTime <= end || o.StartOfUse <= end) && (o.ExpiryByNorm >= begin || o.AutoWriteoffDate >= begin));

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public static ExpenseItem GetExpenseItemForOperation(IUnitOfWork uow, EmployeeIssueOperation operation)
		{
			return uow.Session.QueryOver<ExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id == operation.Id)
				.SingleOrDefault();
		}

		public static IList<EmployeeIssueOperation> GetActualEmployeeOperation(IUnitOfWork uow, EmployeeCard employee, DateTime dateDoc)
		{
			return uow.Session.QueryOver<EmployeeIssueOperation>()
					.Where(x => x.Employee == employee)
					.Where(x => x.AutoWriteoffDate == null)
					.Where(x => x.StartOfUse == null || x.StartOfUse <= dateDoc)
					.Where(x => x.ExpiryByNorm == null || x.ExpiryByNorm < dateDoc)
					.Where(x => x.Issued > 0 && x.Returned < 1)
					.OrderBy(x => x.OperationTime).Asc
					.List();
		}

		public static IList<ReferencedDocument> GetReferencedDocuments(IUnitOfWork uow, int[] operationsIds)
		{
			ReferencedDocument docAlias = null;

			var listIncoms = uow.Session.QueryOver<IncomeItem>()
				.Where(x => x.ReturnFromEmployeeOperation.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.ReturnFromEmployeeOperation.Id).WithAlias(() => docAlias.OpId)
					.Select(i => i.Document.Id).WithAlias(() => docAlias.DocId)
					.Select(() => EmployeeIssueOpReferenceDoc.RetutnedToStock).WithAlias(() => docAlias.DocType)
				)
				.TransformUsing(Transformers.AliasToBean<ReferencedDocument>())
				.List<ReferencedDocument>();

			var listExpense = uow.Session.QueryOver<ExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.EmployeeIssueOperation.Id).WithAlias(() => docAlias.OpId)
					.Select(i => i.ExpenseDoc.Id).WithAlias(() => docAlias.DocId)
					.Select(() => EmployeeIssueOpReferenceDoc.ReceivedFromStock).WithAlias(() => docAlias.DocType)
				)
				.TransformUsing(Transformers.AliasToBean<ReferencedDocument>())
				.List<ReferencedDocument>();

			var listwriteoff = uow.Session.QueryOver<WriteoffItem>()
				.Where(x => x.EmployeeWriteoffOperation.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.EmployeeWriteoffOperation.Id).WithAlias(() => docAlias.OpId)
					.Select(i => i.Document.Id).WithAlias(() => docAlias.DocId)
					.Select(() => EmployeeIssueOpReferenceDoc.WriteOff).WithAlias(() => docAlias.DocType)
				)
				.TransformUsing(Transformers.AliasToBean<ReferencedDocument>())
				.List<ReferencedDocument>();

			var resultList = new List<ReferencedDocument>();
			resultList.AddRange(listIncoms);
			resultList.AddRange(listExpense);
			resultList.AddRange(listwriteoff);

			return resultList;
		}
	}

	public class ReferencedDocument
	{
		public int OpId;
		public int DocId;
		public EmployeeIssueOpReferenceDoc DocType;

		public ReferencedDocument() { }

		public ReferencedDocument(int opId, EmployeeIssueOpReferenceDoc docType, int docId)
		{
			OpId = opId;
			DocType = docType;
			DocId = docId;
		}
	}

	public enum EmployeeIssueOpReferenceDoc
	{
		ReceivedFromStock,
		RetutnedToStock,
		WriteOff
	}
}
