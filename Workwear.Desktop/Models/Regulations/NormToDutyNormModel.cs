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
using Workwear.Domain.Statements;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Models.Regulations {
	public class NormToDutyNormModel {
		private Dictionary<(int employeeId, int normItemId), DutyNormItem> relevantItemsIds = new Dictionary<(int, int), DutyNormItem>();
		private Dictionary<int, ExpenseDutyNormItem> overwritingIds = new Dictionary<int, ExpenseDutyNormItem>();

		private Dictionary<int, DutyNormIssueOperation> dutyNormItemsWithOperationIssuedByDutyNorm =
			new Dictionary<int, DutyNormIssueOperation>();
		
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
					var nextIssue = GetNextIssue(employeeId, item, uow);
					var dutyNormItem = CreateDutyNormItem(newDutyNorm, item, nextIssue);
					uow.Save(dutyNormItem);
					if(!relevantItemsIds.ContainsKey((employee.Id, item.Id)))
						relevantItemsIds.Add((employee.Id, item.Id), dutyNormItem);
				}
				
				var employeeIssueOperations = GetOperationsForEmployeeWithNormItems(employeeId, itemIds, uow);
				IList<DutyNormIssueOperation> dutyNormIssueOperations = new List<DutyNormIssueOperation>();
				foreach(var op in employeeIssueOperations) {
					DutyNormIssueOperation dutyNormIssueOperation = new DutyNormIssueOperation{
						DutyNorm = newDutyNorm
					};
					CreateDutyNormIssueOperation(op, dutyNormIssueOperation);
					uow.Save(dutyNormIssueOperation);
					dutyNormIssueOperations.Add(dutyNormIssueOperation);
				}

				var employeeIssueOperationsIds = employeeIssueOperations.Select(x => x.Id).ToArray();
				var allExpenseDocs = GetExpenseDocs(employeeIssueOperationsIds, uow);
				var allCollectiveExpenseDocs = GetCollectiveExpenseDocs(employeeIssueOperationsIds, uow);
				foreach(var expDoc in allExpenseDocs) {
					ExpenseDutyNorm expenseDutyNormDoc = new ExpenseDutyNorm {
						DutyNorm = newDutyNorm
					};
					CreateExpenseDutyNormDoc(expenseDutyNormDoc, expDoc);
					uow.Save(expenseDutyNormDoc);
					
					foreach(var item in expDoc.Items) {
						var dutyNormIssueOperation = dutyNormIssueOperations.Where(x=>x.DutyNorm == expenseDutyNormDoc.DutyNorm)
							.FirstOrDefault(x=>x.WarehouseOperation == item.WarehouseOperation);
						ExpenseDutyNormItem newExpenseDutyNormItem = new ExpenseDutyNormItem {
							Document = expenseDutyNormDoc,
							WarehouseOperation = item.WarehouseOperation,
							Operation = dutyNormIssueOperation
						};
						uow.Save(newExpenseDutyNormItem);
						
						overwritingIds.Add(item.EmployeeIssueOperation.Id, newExpenseDutyNormItem);
						dutyNormItemsWithOperationIssuedByDutyNorm.Add(newExpenseDutyNormItem.Id, dutyNormIssueOperation);
					}
					uow.Save(expenseDutyNormDoc);
					
					OverWriteIssuanceSheet(expDoc, uow);
					
				}

				foreach(var colExpDoc in allCollectiveExpenseDocs) {
					ExpenseDutyNorm expenseDutyNormDoc = new ExpenseDutyNorm {
						DutyNorm = newDutyNorm
					};
					CreateExpenseDutyNormDoc(expenseDutyNormDoc, colExpDoc);
					uow.Save(expenseDutyNormDoc);

					foreach(var item in colExpDoc.Items) {
						var dutyNormIssueOperation = dutyNormIssueOperations.Where(x => x.DutyNorm == expenseDutyNormDoc.DutyNorm)
							.FirstOrDefault(x => x.WarehouseOperation == item.WarehouseOperation);
						ExpenseDutyNormItem newExpenseDutyNormItem = new ExpenseDutyNormItem {
							Document = expenseDutyNormDoc,
							WarehouseOperation = item.WarehouseOperation,
							Operation = dutyNormIssueOperation
						};
						uow.Save(newExpenseDutyNormItem);
					}

					uow.Save(expenseDutyNormDoc);
					OverWriteIssuanceSheet(colExpDoc, uow);
				}
				
				uow.Commit();
			}
		}

		public DateTime? GetNextIssue(int employeeId, NormItem item, IUnitOfWork uow) {
			var nextIssue = uow.Session
				.Query<EmployeeCardItem>()
				.Where(x => x.EmployeeCard.Id == employeeId)
				.Where(x => x.ActiveNormItem.Id == item.Id)
				.Select(x=>x.NextIssue)
				.FirstOrDefault();
			return nextIssue;
		}
		

		public virtual DutyNormItem CreateDutyNormItem(DutyNorm newDutyNorm, NormItem normItem, DateTime? nextIssue) {
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
		
		public virtual void CreateDutyNormIssueOperation (EmployeeIssueOperation issueOperation, DutyNormIssueOperation dutyNormIssueOperation) 
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

		public IEnumerable<CollectiveExpense> GetCollectiveExpenseDocs(int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			CollectiveExpense collectiveExpenseAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;

			var collectiveExpenseDocs = uow.Session.QueryOver<CollectiveExpense>(() => collectiveExpenseAlias)
				.JoinAlias(x => x.Items, () => collectiveExpenseItemAlias, JoinType.LeftOuterJoin)
				.Where(() => collectiveExpenseItemAlias.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List()
				.Distinct();

			return collectiveExpenseDocs;

		}

		public virtual void CreateExpenseDutyNormDoc(ExpenseDutyNorm expenseDutyNormDoc, CollectiveExpense collectiveExpenseDoc) {

			expenseDutyNormDoc.ResponsibleEmployee = collectiveExpenseDoc.TransferAgent;
			expenseDutyNormDoc.Warehouse = collectiveExpenseDoc.Warehouse;
			expenseDutyNormDoc.CreationDate = DateTime.Now;
			expenseDutyNormDoc.Date = collectiveExpenseDoc.Date;
			expenseDutyNormDoc.Comment = collectiveExpenseDoc.Comment;

		}
		
		// Перенос в ведомости

		public virtual IssuanceSheet GetIssuanceSheet(Expense expenseDoc, IUnitOfWork uow) {
			var issuanceSheet = uow.Session
				.Query<IssuanceSheet>()
				.FirstOrDefault(x => x.Expense.Id == expenseDoc.Id);
			return issuanceSheet;
		}

		public virtual IssuanceSheet GetIssuanceSheet(CollectiveExpense collectiveExpenseDoc, IUnitOfWork uow) {
			var issuanceSheet = uow.Session
				.Query<IssuanceSheet>()
				.FirstOrDefault(x => x.CollectiveExpense.Id == collectiveExpenseDoc.Id);
			return issuanceSheet;
		}

		public virtual void OverWriteIssuanceSheet(Expense expenseDoc, IUnitOfWork uow) {

			IssuanceSheet issuanceSheet = GetIssuanceSheet(expenseDoc, uow);
			var issuanceSheetItems = issuanceSheet.Items.ToList();
			ExpenseDutyNormItem expenseDutyNormItem = null;
			if(issuanceSheetItems.Count == expenseDoc.Items.Count) {
				foreach(var item in issuanceSheetItems) {
					expenseDutyNormItem = overwritingIds[item.IssueOperation.Id];
					item.ExpenseDutyNormItem = expenseDutyNormItem;
					var dutyNormIssueOperation = dutyNormItemsWithOperationIssuedByDutyNorm[expenseDutyNormItem.Id];
					item.DutyNormIssueOperation = dutyNormIssueOperation;
					uow.Save(item);
				}

				issuanceSheet.ExpenseDutyNorm = expenseDutyNormItem?.Document;
			}
			
			foreach(var item in issuanceSheetItems) {
				item.ExpenseItem = null;
				item.IssueOperation = null;
				uow.Save(item);
			}

			issuanceSheet.Expense = null;
			uow.Save(issuanceSheet);
		}
		
		public virtual void OverWriteIssuanceSheet(CollectiveExpense collectiveExpenseDoc, IUnitOfWork uow) {

			IssuanceSheet issuanceSheet = GetIssuanceSheet(collectiveExpenseDoc, uow);
			var issuanceSheetItems = issuanceSheet.Items.ToList();
			
			foreach(var item in issuanceSheetItems) {
				item.CollectiveExpenseItem = null;
				item.IssueOperation = null;
				uow.Save(item);
			}
			issuanceSheet.CollectiveExpense = null;
			uow.Save(issuanceSheet);
		}
		
		// Перенос списаний
		
		public IList<WriteoffItem> GetWriteOffItems(int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			var writeOffOperationsIds = uow.Session.Query<EmployeeIssueOperation>()
				.Where(x => x.IssuedOperation != null && x.IssuedOperation.Id.IsIn(employeeIssueOperationsIds))
				.Select(x => x.Id)
				.ToList();

			var writeOffItems = uow.Session.Query<WriteoffItem>()
				.Where(x => x.EmployeeWriteoffOperation.Id.IsIn(writeOffOperationsIds))
				.ToList();

			return writeOffItems;
		}
		
		
		
}
}
