using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;
using workwear.Domain.Organization;
using workwear.Domain.Stock;

namespace workwear.Repository.Operations
{
	public static class EmployeeIssueRepository
	{
		public static IList<EmployeeIssueOperation> AllOperationsForEmployee(IUnitOfWork uow, EmployeeCard employee, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager)
		{
			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public static Func<IUnitOfWork, EmployeeCard, DateTime, DateTime, IList<EmployeeIssueOperation>> GetOperationsTouchDatesTestGap;
		public static IList<EmployeeIssueOperation> GetOperationsTouchDates(IUnitOfWork uow, EmployeeCard employee, DateTime begin, DateTime end, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null) {
			if(GetOperationsTouchDatesTestGap != null)
				return GetOperationsTouchDatesTestGap(uow, employee, begin, end);

			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee)
				.Where(o => o.StartOfUse <= end && o.ExpiryByNorm >= begin);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public static ExpenseItem GetExpenseItemForOperation(IUnitOfWork uow, EmployeeIssueOperation operation)
		{
			return uow.Session.QueryOver<ExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id == operation.Id)
				.SingleOrDefault();
		}

		public static IList<ReferencedDocument> GetReferencedDocuments(IUnitOfWork uow, int[] operationsIds)
		{
			ReferencedDocument docAlias = null;

			var listIncoms = uow.Session.QueryOver<IncomeItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.EmployeeIssueOperation.Id).WithAlias(() => docAlias.OpId)
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
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.EmployeeIssueOperation.Id).WithAlias(() => docAlias.OpId)
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
