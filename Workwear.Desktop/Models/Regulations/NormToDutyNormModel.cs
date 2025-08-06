using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Models.Regulations {
	public class NormToDutyNormModel {
		private Dictionary<(int employeeId, int normItemId), DutyNormItem> relevantItemsIds = new Dictionary<(int, int), DutyNormItem>();
		
		public virtual void CopyDataFromNorm(int normId, int employeeId) {
			DutyNorm newDutyNorm = new DutyNorm();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Копирование данных из нормы")) {
				var norm = uow.GetById<Norm>(normId);
				var employee = uow.GetById<EmployeeCard>(employeeId);
				var itemIds = norm.Items.Select(i => i.Id).ToArray();
				
				newDutyNorm.Name = norm.Name != null ? $"{norm.Name} ({employee.ShortName})" : $"Дежурная ({employee.ShortName})";
				newDutyNorm.ResponsibleEmployee = employee;
				newDutyNorm.DateFrom = norm.DateFrom;
				newDutyNorm.DateTo = norm.DateTo;
				newDutyNorm.Comment = norm.Comment ?? "";
				newDutyNorm.Subdivision = employee.Subdivision;
				uow.Save(newDutyNorm);
				
				foreach(var item in norm.Items) {
					var nextIssue = uow.Session
						.Query<EmployeeCardItem>()
						.Where(x => x.EmployeeCard.Id == employeeId)
						.Where(x => x.ActiveNormItem.Id == item.Id)
						.Select(x=>x.NextIssue)
						.FirstOrDefault();
					
					var dutyNormItem = CopyNormItem(newDutyNorm, item, nextIssue);
					uow.Save(dutyNormItem);
					if(!relevantItemsIds.ContainsKey((employee.Id, item.Id)))
						relevantItemsIds.Add((employee.Id, item.Id), dutyNormItem);
				}
				
				var employeeIssueOperations = GetOperationsForEmployeeWithNormItems(employeeId, itemIds, uow);
				foreach(var op in employeeIssueOperations) {
					DutyNormIssueOperation dutyNormIssueOperation = new DutyNormIssueOperation{
						DutyNorm = newDutyNorm
					};
					СopyEmployeeIssueOperation(op, dutyNormIssueOperation);
					uow.Save(dutyNormIssueOperation);
				}
				

				var allExpenseDocs = GetExpenseDocs(employeeIssueOperations.Select(x=>x.Id).ToArray(),uow);
				foreach(var expDoc in allExpenseDocs) {
					ExpenseDutyNorm expenseDutyNormDoc = new ExpenseDutyNorm {
						DutyNorm = newDutyNorm
					};
					CreateExpenseDutyNormDoc(expenseDutyNormDoc, expDoc);
					uow.Save(expenseDutyNormDoc);
					
					foreach(var item in expDoc.Items) {
						ExpenseDutyNormItem newExpenseDutyNormItem = new ExpenseDutyNormItem {
							Document = expenseDutyNormDoc,
							WarehouseOperation = item.WarehouseOperation
						};
						var dutyNormIssueOperation = uow.Session.Query<DutyNormIssueOperation>()
							.Where(x => x.DutyNorm == expenseDutyNormDoc.DutyNorm)
							.FirstOrDefault(x=>x.WarehouseOperation == item.WarehouseOperation);
						newExpenseDutyNormItem.Operation = dutyNormIssueOperation;
						uow.Save(newExpenseDutyNormItem);
					}
					uow.Save(expenseDutyNormDoc);
					
				}
				
				uow.Commit();
			}
		}

		public virtual DutyNormItem CopyNormItem(DutyNorm newDutyNorm, NormItem normItem, DateTime? nextIssue) {
			DutyNormItem newDutyNormItem = new DutyNormItem();
			
			newDutyNormItem.DutyNorm = newDutyNorm;
			newDutyNormItem.ProtectionTools = normItem.ProtectionTools;
			newDutyNormItem.Amount = normItem.Amount;
			
			switch(normItem.NormPeriod) {
				case NormPeriodType.Year:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Year; 
				break;
				case NormPeriodType.Month:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Month;
					break;
				case NormPeriodType.Wearout:
				case NormPeriodType.Shift:
				case NormPeriodType.Duty:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Wearout;
				break;
			}
			
			newDutyNormItem.PeriodCount = normItem.PeriodCount;
			newDutyNormItem.NormParagraph = normItem.NormParagraph;
			newDutyNormItem.Comment = normItem.Comment;
			newDutyNormItem.Graph = new IssueGraph();
			newDutyNormItem.NextIssue = nextIssue;
			
			return newDutyNormItem;
		}
		
		public virtual void СopyEmployeeIssueOperation (EmployeeIssueOperation issueOperation, DutyNormIssueOperation dutyNormIssueOperation) 
		{
			dutyNormIssueOperation.DutyNormItem = relevantItemsIds[(dutyNormIssueOperation.DutyNorm.ResponsibleEmployee.Id, issueOperation.NormItem.Id)];
			dutyNormIssueOperation.OperationTime = issueOperation.OperationTime;
			dutyNormIssueOperation.Nomenclature = issueOperation.Nomenclature;
			dutyNormIssueOperation.ProtectionTools = issueOperation.ProtectionTools;
			dutyNormIssueOperation.WearSize = issueOperation.WearSize;
			dutyNormIssueOperation.Height = issueOperation.Height;
			dutyNormIssueOperation.WearPercent = issueOperation.WearPercent;
			dutyNormIssueOperation.AutoWriteoffDate = issueOperation.AutoWriteoffDate;
			dutyNormIssueOperation.Issued = issueOperation.Issued;
			dutyNormIssueOperation.WarehouseOperation = issueOperation.WarehouseOperation;
		}

		public IList<EmployeeIssueOperation> GetOperationsForEmployeeWithNormItems(int employeeId, int[] normItemsIds, IUnitOfWork uow) {
			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.NormItem.Id.IsIn(normItemsIds))
				.Where(o => o.Employee.Id == employeeId)
				.Where(o => o.Issued > 0);
			return query.List();

		}
		
		public IEnumerable<Expense> GetExpenseDocs (int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			Expense expenseAlias = null;
			ExpenseItem expenseItemAlias = null;

			var expenseDocs = uow.Session.QueryOver<Expense>(() => expenseAlias)
				.JoinAlias(x => x.Items, () => expenseItemAlias, JoinType.LeftOuterJoin)
				.Where(() => expenseItemAlias.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List()
				.Distinct();

			return expenseDocs;

		}
		public virtual void CreateExpenseDutyNormDoc(ExpenseDutyNorm expenseDutyNormDoc, Expense expenseDoc) {
			
			expenseDutyNormDoc.ResponsibleEmployee = expenseDoc.Employee;
			expenseDutyNormDoc.Warehouse = expenseDoc.Warehouse;
			expenseDutyNormDoc.CreationDate = DateTime.Now;
			expenseDutyNormDoc.Date = expenseDoc.Date;
			expenseDutyNormDoc.Comment = expenseDoc.Comment;
			
		}
		// Для коллективной выдачи
		
		/*public IList<CollectiveExpense> GetCollectiveExpenseDocs(Norm norm, IUnitOfWork uow) {
			
			CollectiveExpense collectiveExpenseAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			Norm normAlias = null;
			NormItem normItemAlias = null;

			var allCollectiveExpenseDocsForNorm = uow.Session.QueryOver<CollectiveExpense>(() => collectiveExpenseAlias)
				.JoinAlias(x => x.Items, () => collectiveExpenseItemAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => collectiveExpenseItemAlias,
					() => collectiveExpenseItemAlias.EmployeeIssueOperation.Id == employeeIssueOperationAlias.Id)
				.JoinAlias(() => employeeIssueOperationAlias.NormItem, () => normItemAlias)
				.JoinEntityAlias(() => normItemAlias, () => normItemAlias.Norm.Id == normAlias.Id)
				.Where(() => normAlias.Id == norm.Id)
				.List();

			return allCollectiveExpenseDocsForNorm;

		}*/ 
	}
}
