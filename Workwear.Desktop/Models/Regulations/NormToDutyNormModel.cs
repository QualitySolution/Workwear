using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Models.Regulations {
	public class NormToDutyNormModel {
		public virtual void CopyDataFromNorm(int normId) {
			Dictionary<int, DutyNormIssueOperation> relevantOperations = new Dictionary<int, DutyNormIssueOperation>();
			IList<ExpenseItem> removingExpenseItems = new List<ExpenseItem>();
			IList<CollectiveExpenseItem> removingCollectiveExpenseItems = new List<CollectiveExpenseItem>();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Копирование данных из нормы")) {
				var norm = uow.GetById<Norm>(normId);
				var itemIds = norm.Items.Select(i => i.Id).ToArray();
				var employees = norm.Employees.ToList();
				 
				foreach(var employee in norm.Employees) {
					Dictionary<(int employeeId, int normItemId), DutyNormItem> relevantItemsIds = new Dictionary<(int, int), DutyNormItem>();
					Dictionary<int, ExpenseDutyNormItem> overwritingIds = new Dictionary<int, ExpenseDutyNormItem>();
					Dictionary<int, DutyNormIssueOperation> dutyNormItemsWithOperationIssuedByDutyNorm =
						new Dictionary<int, DutyNormIssueOperation>();
					Dictionary<int, DutyNormIssueOperation> relevantIssueOperations =
						new Dictionary<int, DutyNormIssueOperation>();
					DutyNorm newDutyNorm = new DutyNorm();
					var employeeIssueOperations = GetOperationsForEmployeeWithNormItems(employee.Id, itemIds, uow);
					newDutyNorm.Name = norm.Name != null ? $"{norm.Name} ({employee.ShortName})" : $"Дежурная ({employee.ShortName})";
					newDutyNorm.ResponsibleEmployee = employee;
					newDutyNorm.DateFrom = norm.DateFrom;
					newDutyNorm.DateTo = norm.DateTo;
					newDutyNorm.Comment = norm.Comment ?? "";
					newDutyNorm.Subdivision = employee.Subdivision;
					uow.Save(newDutyNorm);

					foreach(var item in norm.Items) {
						var nextIssue = GetNextIssue(employee.Id, item, uow);
						var dutyNormItem = CreateDutyNormItem(newDutyNorm, item, nextIssue);
						uow.Save(dutyNormItem);
						if(!relevantItemsIds.ContainsKey((employee.Id, item.Id)))
							relevantItemsIds.Add((employee.Id, item.Id), dutyNormItem);
					}
					
					foreach(var op in employeeIssueOperations) {
						DutyNormIssueOperation dutyNormIssueOperation = new DutyNormIssueOperation {
							DutyNorm = newDutyNorm
						};
						CreateDutyNormIssueOperation(op, dutyNormIssueOperation, relevantItemsIds);
						uow.Save(dutyNormIssueOperation);
						relevantOperations.Add(op.WarehouseOperation.Id, dutyNormIssueOperation);
						relevantIssueOperations.Add(op.Id, dutyNormIssueOperation);
					}

					OverWriteBarcodeOperations(relevantIssueOperations, uow);
					
					var employeeIssueOperationsIds = employeeIssueOperations.Select(x => x.Id).ToArray();
					var allExpenseDocsForEmployeeWithItems = GetExpenseDocsForEmployee(employeeIssueOperationsIds, uow).ToArray();
					var allCollectiveExpenseDocsForEmployeeWithItems = GetCollectiveExpenseDocsForEmployee(employeeIssueOperationsIds, uow).ToArray();
					foreach(var expDocIt in allExpenseDocsForEmployeeWithItems) {
						var expDoc = expDocIt.Key;
						var items = expDocIt.Value;
						ExpenseDutyNorm expenseDutyNormDoc = new ExpenseDutyNorm {
							DutyNorm = newDutyNorm
						};
						CreateExpenseDutyNormDoc(expenseDutyNormDoc, expDoc);
						uow.Save(expenseDutyNormDoc);

						foreach(var item in items) {
							var dutyNormIssueOperation = relevantOperations[item.WarehouseOperation.Id];
							ExpenseDutyNormItem newExpenseDutyNormItem = new ExpenseDutyNormItem {
								Document = expenseDutyNormDoc,
								WarehouseOperation = item.WarehouseOperation,
								Operation = dutyNormIssueOperation
							};
							uow.Save(newExpenseDutyNormItem);

							overwritingIds.Add(item.EmployeeIssueOperation.Id, newExpenseDutyNormItem);
							dutyNormItemsWithOperationIssuedByDutyNorm.Add(newExpenseDutyNormItem.Id, dutyNormIssueOperation);
							removingExpenseItems.Add(item);
						}
						uow.Save(expenseDutyNormDoc);
						OverWriteIssuanceSheet(expDoc, overwritingIds, dutyNormItemsWithOperationIssuedByDutyNorm, uow);
						
						expDoc.Items.RemoveAll(item => removingExpenseItems.Contains(item));
						if(expDoc.Items.Count == 0)
							uow.Delete(expDoc);
					}

					foreach(var colExpDocIt in allCollectiveExpenseDocsForEmployeeWithItems) {
						var colExpDoc = colExpDocIt.Key;
						var items = colExpDocIt.Value;
						ExpenseDutyNorm expenseDutyNormDoc = new ExpenseDutyNorm {
							DutyNorm = newDutyNorm
						};
						CreateExpenseDutyNormDoc(expenseDutyNormDoc, colExpDoc);
						uow.Save(expenseDutyNormDoc);
						
						foreach(var item in items) {
							var dutyNormIssueOperation = relevantOperations[item.WarehouseOperation.Id];
							ExpenseDutyNormItem newExpenseDutyNormItem = new ExpenseDutyNormItem {
								Document = expenseDutyNormDoc,
								WarehouseOperation = item.WarehouseOperation,
								Operation = dutyNormIssueOperation
							};
							uow.Save(newExpenseDutyNormItem);

							removingCollectiveExpenseItems.Add(item);
						}
						
						uow.Save(expenseDutyNormDoc);
						OverWriteIssuanceSheet(colExpDoc, uow);

						colExpDoc.Items.RemoveAll(item => removingCollectiveExpenseItems.Contains(item));
						if(colExpDoc.Items.Count == 0)
							uow.Delete(colExpDoc);
					}

					OverWriteWriteOffDocs(employeeIssueOperationsIds, overwritingIds, dutyNormItemsWithOperationIssuedByDutyNorm, uow);
					
				}
				RemoveNorm(norm, uow);
				foreach(var emp in employees)
					emp.UpdateWorkwearItems();
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
		
		public virtual void CreateDutyNormIssueOperation (
			EmployeeIssueOperation issueOperation, 
			DutyNormIssueOperation dutyNormIssueOperation,
			Dictionary<(int employeeId, int normItemId), DutyNormItem> relevantItemsIds) 
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
				.Where(o => o.Issued > 0)
				.Where(o => o.ManualOperation == false);
			return query.List();

		}
		
		public Dictionary<Expense, List<ExpenseItem>> GetExpenseDocsForEmployee (int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			
			var expenseItems = uow.Session.QueryOver<ExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List();
			
			var expenseDocs = expenseItems
				.GroupBy(x => x.ExpenseDoc)
				.ToDictionary(g => g.Key, g => g.ToList());

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

		public Dictionary<CollectiveExpense, List<CollectiveExpenseItem>> GetCollectiveExpenseDocsForEmployee(int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			
			var collectiveExpenseItems = uow.Session.QueryOver<CollectiveExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List();
			var collectiveExpenseDocs = collectiveExpenseItems
				.GroupBy(x => x.Document)
				.ToDictionary(g => g.Key, g => g.ToList());

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

		public virtual void OverWriteIssuanceSheet(
			Expense expenseDoc, 
			Dictionary<int, ExpenseDutyNormItem> overwritingIds,
			Dictionary<int, DutyNormIssueOperation> dutyNormItemsWithOperationIssuedByDutyNorm,
			IUnitOfWork uow) {

			IssuanceSheet issuanceSheet = GetIssuanceSheet(expenseDoc, uow);
			if(issuanceSheet == null)
				return;
			var issueOperationsIds = overwritingIds.Keys.ToArray();
			var issuanceSheetItems = issuanceSheet.Items.ToList();
			var changingIssuanceSheetItems = issuanceSheetItems
				.Where(x => issueOperationsIds.Contains(x.IssueOperation.Id));
			ExpenseDutyNormItem expenseDutyNormItem = null;
			if(issuanceSheetItems.Count == expenseDoc.Items.Count) {
				foreach(var item in changingIssuanceSheetItems) {
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
			if(issuanceSheet == null)
				return;
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
				.Where(x => x.IssuedOperation != null)
				.Where(x => employeeIssueOperationsIds.Contains(x.IssuedOperation.Id))
				.Select(x => x.Id)
				.ToList();

			var writeOffItems = uow.Session.Query<WriteoffItem>()
				.Where(x => writeOffOperationsIds.Contains(x.EmployeeWriteoffOperation.Id))
				.ToList();

			return writeOffItems;
		}

		public virtual void OverWriteWriteOffDocs
			(int[] employeeIssueOperationsIds,
			Dictionary<int, ExpenseDutyNormItem> overwritingIds,
			Dictionary<int, DutyNormIssueOperation> dutyNormItemsWithOperationIssuedByDutyNorm,
			IUnitOfWork uow) {
			var writeOffItems = GetWriteOffItems(employeeIssueOperationsIds, uow);
			foreach(var item in writeOffItems) 
			{
				var expenseDutyNormItem = overwritingIds[item.EmployeeWriteoffOperation.IssuedOperation.Id];
				var dutyNormIssueOperation = dutyNormItemsWithOperationIssuedByDutyNorm[expenseDutyNormItem.Id];
				var removingWriteOffOperation = item.EmployeeWriteoffOperation;
				item.DutyNormWriteOffOperation = dutyNormIssueOperation;
				item.EmployeeWriteoffOperation = null;
				uow.Save(item);
				uow.Delete(removingWriteOffOperation);
			}
		}
		public virtual void RemoveNorm(Norm norm, IUnitOfWork uow) {
			norm?.Employees.Clear();
			norm?.Posts.Clear();
			norm?.Items.Clear();
			uow.Delete(norm);
		}

		private void OverWriteBarcodeOperations(
			Dictionary<int, DutyNormIssueOperation> relevantIssueOperations, 
			IUnitOfWork uow) {
			var employeeIssueOperationsIds = relevantIssueOperations.Keys.ToArray();
			var barcodeOperations = uow.Session.Query<BarcodeOperation>()
				.Where(x => employeeIssueOperationsIds.Contains(x.EmployeeIssueOperation.Id))
				.ToList();
			if(barcodeOperations.IsEmpty())
				return;
			foreach(var op in barcodeOperations) {
				op.DutyNormIssueOperation = relevantIssueOperations[op.EmployeeIssueOperation.Id];
				op.EmployeeIssueOperation = null;
				uow.Save(op);
			}
		}
	}
}
