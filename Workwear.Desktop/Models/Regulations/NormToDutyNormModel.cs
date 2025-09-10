using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;

namespace Workwear.Models.Regulations {
	public class NormToDutyNormModel {
		private readonly IInteractiveService interactive;
		private readonly IProgressBarDisplayable progressBar;
		private EmployeeIssueRepository employeeIssueRepository;
		public NormToDutyNormModel(
			IInteractiveService interactive, 
			IProgressBarDisplayable progressBar,
			EmployeeIssueRepository employeeIssueRepository) {
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.progressBar = progressBar;
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
		}
		public virtual void CopyNormToDutyNorm(int normId) {
			Dictionary<int, DutyNormIssueOperation> dutyNormIssueOperationByWarehouseOperation = new Dictionary<int, DutyNormIssueOperation>();
			IList<ExpenseItem> removingExpenseItems = new List<ExpenseItem>();
			IList<CollectiveExpenseItem> removingCollectiveExpenseItems = new List<CollectiveExpenseItem>();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Копирование обычной нормы в дежурную")) {
				employeeIssueRepository.RepoUow = uow;
				var norm = uow.GetById<Norm>(normId);
				var employees = norm.Employees.ToList();
				
				if(employees.Count >= 7)
					if(!interactive.Question(
						   $"При выполнении данного действия будут созданы дежурные нормы в количестве: {employees.Count}. " +
						   $"Продолжить?"))
						return;
				
				var itemIds = norm.Items.Select(i => i.Id).ToArray();
				var posts = norm.Posts.ToList();
				
				progressBar.Start(employees.Count + posts.Count, text: "Переносим норму в дежурные");
				if(employees.IsEmpty() && posts.IsEmpty()) {
					DutyNorm newDutyNorm = new DutyNorm();
					CopyNormData(norm, newDutyNorm);
					uow.Save(newDutyNorm);
					CreateDutyNormName(norm, newDutyNorm, uow);
					progressBar.Add(text: $"Создаем новую дежурную норму {newDutyNorm.Name}");
					
					foreach(var item in norm.Items) {
						var dutyNormItem = CreateDutyNormItem(newDutyNorm, item);
						uow.Save(dutyNormItem);
					}
				}

				if(posts.IsNotEmpty()) {
					foreach(var post in norm.Posts) {
						DutyNorm newDutyNorm = new DutyNorm();
						CopyNormData(norm, newDutyNorm, null, post);
						uow.Save(newDutyNorm);
						CreateDutyNormName(norm, newDutyNorm, post, uow);
						progressBar.Add(text: $"Создаем новую дежурную норму {newDutyNorm.Name} для должности: {post.Name}");
						foreach(var item in norm.Items) {
							var dutyNormItem = CreateDutyNormItem(newDutyNorm, item);
							uow.Save(dutyNormItem);
						}
					}
				}
				 
				foreach(var employee in norm.Employees) {
					Dictionary<(int employeeId, int normItemId), DutyNormItem> dutyNormItemByEmployeeAndNormItem = new Dictionary<(int, int), DutyNormItem>();
					Dictionary<int, ExpenseDutyNormItem> expDutyNormItemByIssueOperation = new Dictionary<int, ExpenseDutyNormItem>();
					Dictionary<int, DutyNormIssueOperation> dutyNormIssueOperationByExpDutyNormItem =
						new Dictionary<int, DutyNormIssueOperation>();
					Dictionary<int, DutyNormIssueOperation> dutyNormIssueOperationByIssueOperation =
						new Dictionary<int, DutyNormIssueOperation>();
					Dictionary<int, DutyNormIssueOperation> dutyNormWriteOffOperationByEmployeeWriteOffOperation =
						new Dictionary<int, DutyNormIssueOperation>();
					var employeeIssueOperations = employeeIssueRepository.GetIssueOperationsForEmployeeWithNormItems(employee.Id, itemIds, uow);
					var writeOffOperations = GetWriteOffOperationsForEmployeeByNorm(employeeIssueOperations, uow);
					DutyNorm newDutyNorm = new DutyNorm();
					CopyNormData(norm, newDutyNorm, employee);
					uow.Save(newDutyNorm);
					CreateDutyNormName(norm, newDutyNorm, uow, employee);
					progressBar.Add(text: $"Создаем новую дежурную норму {newDutyNorm.Name} для сотрудника {employee.ShortName}");
					foreach(var item in norm.Items) {
						var nextIssue = GetNextIssue(employee.Id, item, uow);
						var dutyNormItem = CreateDutyNormItem(newDutyNorm, item, nextIssue);
						uow.Save(dutyNormItem);
						dutyNormItemByEmployeeAndNormItem.Add((employee.Id, item.Id), dutyNormItem);
					}
					progressBar.Add(text: $"Создаем операции выдачи по дежурной норме {newDutyNorm.Name}");
					foreach(var op in employeeIssueOperations) {
						DutyNormIssueOperation dutyNormIssueOperation = new DutyNormIssueOperation {
							DutyNorm = newDutyNorm
						};
						CreateDutyNormIssueOperation(op, dutyNormIssueOperation, dutyNormItemByEmployeeAndNormItem);
						uow.Save(dutyNormIssueOperation);
						dutyNormIssueOperationByWarehouseOperation.Add(op.WarehouseOperation.Id, dutyNormIssueOperation);
						dutyNormIssueOperationByIssueOperation.Add(op.Id, dutyNormIssueOperation);
					}
					progressBar.Add(text: $"Создаем операции списания по дежурной норме {newDutyNorm.Name}");
					foreach(var writeOffOp in writeOffOperations) {
						DutyNormIssueOperation dutyNormIssueOperation = new DutyNormIssueOperation {
							DutyNorm = newDutyNorm
						};
						CreateDutyNormIssueOperation(writeOffOp, dutyNormIssueOperation);
						uow.Save(dutyNormIssueOperation);
						dutyNormWriteOffOperationByEmployeeWriteOffOperation.Add(writeOffOp.Id, dutyNormIssueOperation);
						var issuedOperation = dutyNormIssueOperationByIssueOperation[writeOffOp.IssuedOperation.Id];
						dutyNormIssueOperation.IssuedOperation = issuedOperation;
						dutyNormIssueOperation.DutyNormItem = issuedOperation.DutyNormItem;
						uow.Save(dutyNormIssueOperation);
					}

					UpdateBarcodeOperations(dutyNormIssueOperationByIssueOperation, uow);
					
					var employeeIssueOperationsIds = employeeIssueOperations.Select(x => x.Id).ToArray();
					var allExpenseDocsForEmployeeWithItems = GetExpenseDocsForEmployee(employeeIssueOperationsIds, uow).ToArray();
					var allCollectiveExpenseDocsForEmployeeWithItems = GetCollectiveExpenseDocsForEmployee(employeeIssueOperationsIds, uow).ToArray();
					foreach(var expDocIt in allExpenseDocsForEmployeeWithItems) {
						var expDoc = expDocIt.Key;
						var items = expDocIt.Value;
						progressBar.Add(text: "Создаем новый документ выдачи по дежурной норме на основании документа индивидуальной выдачи №" +
						                      $"{expDoc.DocNumber ?? expDoc.Id.ToString()}");
						ExpenseDutyNorm expenseDutyNormDoc = new ExpenseDutyNorm {
							DutyNorm = newDutyNorm
						};
						CreateExpenseDutyNormDoc(expenseDutyNormDoc, expDoc);
						uow.Save(expenseDutyNormDoc);
						expenseDutyNormDoc.Comment +=
							$"{(expDoc.DocNumber != null ? $" Номер документа выдачи сотруднику: {expDoc.DocNumber}" : "")}" +
							$"{(expDoc.CreationDate != null ? $" Дата создания документа выдачи сотруднику: {expDoc.CreationDate?.ToString("dd.MM.yyyy")}" : "")}" +
							$" Документ выдачи сотруднику был создан пользователем: {expDoc.CreatedbyUser.Name}"; 

						foreach(var item in items) {
							var dutyNormIssueOperation = dutyNormIssueOperationByWarehouseOperation[item.WarehouseOperation.Id];
							ExpenseDutyNormItem newExpenseDutyNormItem = new ExpenseDutyNormItem {
								Document = expenseDutyNormDoc,
								WarehouseOperation = item.WarehouseOperation,
								Operation = dutyNormIssueOperation
							};
							uow.Save(newExpenseDutyNormItem);

							expDutyNormItemByIssueOperation.Add(item.EmployeeIssueOperation.Id, newExpenseDutyNormItem);
							dutyNormIssueOperationByExpDutyNormItem.Add(newExpenseDutyNormItem.Id, dutyNormIssueOperation);
							removingExpenseItems.Add(item);
						}
						uow.Save(expenseDutyNormDoc);
						UpdateIssuanceSheet(expDoc, expDutyNormItemByIssueOperation, dutyNormIssueOperationByExpDutyNormItem, uow);
						
						progressBar.Add(text: $"Очищение документа индивидуальной выдачи №{expDoc.DocNumber ?? expDoc.Id.ToString()}");
						expDoc.Items.RemoveAll(item => removingExpenseItems.Contains(item));
						if(expDoc.Items.Count == 0) {
							progressBar.Add(text: $"Удаление документа индивидуальной выдачи №{expDoc.DocNumber ?? expDoc.Id.ToString()}");
							uow.Delete(expDoc);
						}
							
					}

					foreach(var colExpDocIt in allCollectiveExpenseDocsForEmployeeWithItems) {
						var colExpDoc = colExpDocIt.Key;
						var items = colExpDocIt.Value;
						progressBar.Add(text: "Создаем новый документ выдачи по дежурной норме на основании документа коллективной выдачи №" +
						                      $"{colExpDoc.DocNumber ?? colExpDoc.Id.ToString()}");
						ExpenseDutyNorm expenseDutyNormDoc = new ExpenseDutyNorm {
							DutyNorm = newDutyNorm
						};
						CreateExpenseDutyNormDoc(expenseDutyNormDoc, colExpDoc);
						uow.Save(expenseDutyNormDoc);
						expenseDutyNormDoc.Comment +=
							$"{(colExpDoc.DocNumber != null ? $" Номер документа выдачи сотруднику: {colExpDoc.DocNumber}" : "")}" +
							$"{(colExpDoc.CreationDate != null ? $" Дата создания документа выдачи сотруднику: {colExpDoc.CreationDate?.ToString("dd.MM.yyyy")}" : "")}" +
							$" Документ выдачи сотруднику был создан пользователем: {colExpDoc.CreatedbyUser.Name}"; 
						foreach(var item in items) {
							var dutyNormIssueOperation = dutyNormIssueOperationByWarehouseOperation[item.WarehouseOperation.Id];
							ExpenseDutyNormItem newExpenseDutyNormItem = new ExpenseDutyNormItem {
								Document = expenseDutyNormDoc,
								WarehouseOperation = item.WarehouseOperation,
								Operation = dutyNormIssueOperation
							};
							uow.Save(newExpenseDutyNormItem);

							removingCollectiveExpenseItems.Add(item);
						}
						
						uow.Save(expenseDutyNormDoc);
						UpdateIssuanceSheet(colExpDoc, uow);

						progressBar.Add(text: $"Очищение документа коллективной выдачи №{colExpDoc.DocNumber ?? colExpDoc.Id.ToString()}");
						colExpDoc.Items.RemoveAll(item => removingCollectiveExpenseItems.Contains(item));
						if(colExpDoc.Items.Count == 0) {
							progressBar.Add(text: $"Удаление документа коллективной выдачи №{colExpDoc.DocNumber ?? colExpDoc.Id.ToString()}");
							uow.Delete(colExpDoc);
						}
					}
					UpdateWriteWriteOffDocs(employeeIssueOperationsIds, dutyNormWriteOffOperationByEmployeeWriteOffOperation, newDutyNorm, uow);
					
				}
				RemoveNorm(norm, uow);
				foreach(var emp in employees) {
					progressBar.Add(text: $"Пересчитываем потребности для {emp.ShortName}");
					emp.UpdateWorkwearItems();
				}
				progressBar.Close();	
				uow.Commit();
			}
		}

		#region Создание дежурной нормы
		private DateTime? GetNextIssue(int employeeId, NormItem item, IUnitOfWork uow) {
			var nextIssue = uow.Session
				.Query<EmployeeCardItem>()
				.Where(x => x.EmployeeCard.Id == employeeId)
				.Where(x => x.ActiveNormItem.Id == item.Id)
				.Select(x=>x.NextIssue)
				.FirstOrDefault();
			return nextIssue;
		}
		private void CopyNormData(Norm norm, DutyNorm newDutyNorm, EmployeeCard employee = null, Post post = null) {
			newDutyNorm.ResponsibleEmployee = employee;
			newDutyNorm.DateFrom = norm.DateFrom;
			newDutyNorm.DateTo = norm.DateTo;
			newDutyNorm.Comment = norm.Comment + $" Создана переносом из нормы №{norm.Id} {norm.Name}";
			newDutyNorm.Subdivision = employee?.Subdivision ?? post?.Subdivision;
		}

		private void CreateDutyNormName(Norm norm, DutyNorm newDutyNorm, IUnitOfWork uow, EmployeeCard employee = null) {
			newDutyNorm.Name = norm.Name != null 
				? $"{norm.Name} {(employee != null ? $"({employee.ShortName})" : "")}" 
				: $"Дежурная {(employee != null ? $"({employee.ShortName})" : $"норма №{newDutyNorm.Id}")}";
			uow.Save(newDutyNorm);
		}

		private void CreateDutyNormName(Norm norm, DutyNorm newDutyNorm, Post post, IUnitOfWork uow) {
			newDutyNorm.Name = norm.Name != null
				? $"{norm.Name} ({post.Name})" : $"Дежурная ({post.Name})";
			uow.Save(newDutyNorm);
		}

		private DutyNormItem CreateDutyNormItem(DutyNorm newDutyNorm, NormItem normItem, DateTime? nextIssue = null) {
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
		#endregion

		#region Выдачи и списания
		private void CreateDutyNormIssueOperation (
			EmployeeIssueOperation issueOperation, 
			DutyNormIssueOperation dutyNormIssueOperation,
			Dictionary<(int employeeId, int normItemId), DutyNormItem> dutyNormItemByEmployeeAndNormItem = null) 
		{
			if(dutyNormItemByEmployeeAndNormItem != null)
				dutyNormIssueOperation.DutyNormItem = dutyNormItemByEmployeeAndNormItem[(dutyNormIssueOperation.DutyNorm.ResponsibleEmployee.Id, issueOperation.NormItem.Id)];
			dutyNormIssueOperation.OperationTime = issueOperation.OperationTime;
			dutyNormIssueOperation.Nomenclature = issueOperation.Nomenclature;
			dutyNormIssueOperation.WearSize = issueOperation.WearSize;
			dutyNormIssueOperation.Height = issueOperation.Height;
			dutyNormIssueOperation.WearPercent = issueOperation.WearPercent;
			dutyNormIssueOperation.Issued = issueOperation.Issued;
			dutyNormIssueOperation.Returned = issueOperation.Returned;
			dutyNormIssueOperation.UseAutoWriteoff = issueOperation.UseAutoWriteoff;
			dutyNormIssueOperation.AutoWriteoffDate = issueOperation.AutoWriteoffDate;
			dutyNormIssueOperation.ProtectionTools = issueOperation.ProtectionTools;
			dutyNormIssueOperation.StartOfUse = issueOperation.StartOfUse;
			dutyNormIssueOperation.ExpiryByNorm = issueOperation.ExpiryByNorm;
			dutyNormIssueOperation.WarehouseOperation = issueOperation.WarehouseOperation;
			dutyNormIssueOperation.Comment = issueOperation.Comment;
		}

		private IList<EmployeeIssueOperation> GetWriteOffOperationsForEmployeeByNorm(
			IList<EmployeeIssueOperation> issueOperations,
			IUnitOfWork uow) {
			var writeOffOperations = uow.Session.Query<EmployeeIssueOperation>()
				.Where(o => issueOperations.Contains(o.IssuedOperation))
				.ToList();
			return writeOffOperations;
		}
		#endregion

		#region Документы выдачи
		private Dictionary<Expense, List<ExpenseItem>> GetExpenseDocsForEmployee (int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			
			var expenseItems = uow.Session.QueryOver<ExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List();
			
			var expenseDocs = expenseItems
				.GroupBy(x => x.ExpenseDoc)
				.ToDictionary(g => g.Key, g => g.ToList());

			return expenseDocs;
		}
		private void CreateExpenseDutyNormDoc(ExpenseDutyNorm expenseDutyNormDoc, Expense expenseDoc) {
			expenseDutyNormDoc.ResponsibleEmployee = expenseDoc.Employee;
			expenseDutyNormDoc.Warehouse = expenseDoc.Warehouse;
			expenseDutyNormDoc.CreationDate = DateTime.Now;
			expenseDutyNormDoc.Date = expenseDoc.Date;
			expenseDutyNormDoc.Comment = expenseDoc.Comment;
		}
		private Dictionary<CollectiveExpense, List<CollectiveExpenseItem>> GetCollectiveExpenseDocsForEmployee(int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			
			var collectiveExpenseItems = uow.Session.QueryOver<CollectiveExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List();
			var collectiveExpenseDocs = collectiveExpenseItems
				.GroupBy(x => x.Document)
				.ToDictionary(g => g.Key, g => g.ToList());

			return collectiveExpenseDocs;
		}

		private void CreateExpenseDutyNormDoc(ExpenseDutyNorm expenseDutyNormDoc, CollectiveExpense collectiveExpenseDoc) {
			expenseDutyNormDoc.ResponsibleEmployee = collectiveExpenseDoc.TransferAgent;
			expenseDutyNormDoc.Warehouse = collectiveExpenseDoc.Warehouse;
			expenseDutyNormDoc.CreationDate = DateTime.Now;
			expenseDutyNormDoc.Date = collectiveExpenseDoc.Date;
			expenseDutyNormDoc.Comment = collectiveExpenseDoc.Comment;
		}
		#endregion

		#region Ведомости
		private IssuanceSheet GetIssuanceSheet(Expense expenseDoc, IUnitOfWork uow) {
			var issuanceSheet = uow.Session
				.Query<IssuanceSheet>()
				.FirstOrDefault(x => x.Expense.Id == expenseDoc.Id);
			return issuanceSheet;
		}

		private IssuanceSheet GetIssuanceSheet(CollectiveExpense collectiveExpenseDoc, IUnitOfWork uow) {
			var issuanceSheet = uow.Session
				.Query<IssuanceSheet>()
				.FirstOrDefault(x => x.CollectiveExpense.Id == collectiveExpenseDoc.Id);
			return issuanceSheet;
		}

		private void UpdateIssuanceSheet(
			Expense expenseDoc, 
			Dictionary<int, ExpenseDutyNormItem> expDutyNormItemByIssueOperation,
			Dictionary<int, DutyNormIssueOperation> dutyNormIssueOperationByExpDutyNormItem,
			IUnitOfWork uow) {

			IssuanceSheet issuanceSheet = GetIssuanceSheet(expenseDoc, uow);
			if(issuanceSheet?.Expense == null)
				return;
			var issueOperationsIds = expDutyNormItemByIssueOperation.Keys.ToArray();
			var issuanceSheetItems = issuanceSheet.Items.ToList();
			var changingIssuanceSheetItems = issuanceSheetItems
				.Where(x => issueOperationsIds.Contains(x.IssueOperation.Id));
			ExpenseDutyNormItem expenseDutyNormItem = null;
			if(issuanceSheetItems.Count == expenseDoc.Items.Count) {
				progressBar.Add(text: "Переносим операции выдачи по дежурной норме в ведомость №" +
				                           $"{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()}");
				foreach(var item in changingIssuanceSheetItems) {
					expenseDutyNormItem = expDutyNormItemByIssueOperation[item.IssueOperation.Id];
					item.ExpenseDutyNormItem = expenseDutyNormItem;
					var dutyNormIssueOperation = dutyNormIssueOperationByExpDutyNormItem[expenseDutyNormItem.Id];
					item.DutyNormIssueOperation = dutyNormIssueOperation;
					uow.Save(item);
				}
				progressBar.Add(text: "Переносим ссылку на документ выдачи по дежурной норме №" +
				                      $"{expenseDutyNormItem?.Document.DocNumber ?? expenseDutyNormItem?.Document.Id.ToString()} " +
				                      $"в ведомости №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()}");

				issuanceSheet.ExpenseDutyNorm = expenseDutyNormItem?.Document;
			}
			else {
				progressBar.Add(text: $"Отвязываем ведомость №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()} " +
				                      $"от документа выдачи №{issuanceSheet.Expense.DocNumber ?? issuanceSheet.Expense.Id.ToString()}");
				expenseDoc.Comment += $" Ведомость №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()} отвязана от документа выдачи №" +
				                      $"{issuanceSheet.Expense.DocNumber ?? issuanceSheet.Expense.Id.ToString()}";
			}
				
			if(issuanceSheetItems.IsNotEmpty())
				progressBar.Add(text: $"Очищаем ведомость №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()} " +
				                      "от операций выдачи сотрудникам");
			foreach(var item in issuanceSheetItems) {
				item.ExpenseItem = null;
				item.IssueOperation = null;
				uow.Save(item);
			}

			progressBar.Add(text: "Очищаем ссылку на документ индивидуальной выдачи №" +
			                      $"{issuanceSheet.Expense.DocNumber ?? issuanceSheet.Expense.Id.ToString()} " +
			                      $"в ведомости №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()}");
			issuanceSheet.Expense = null;
			uow.Save(expenseDoc);
			uow.Save(issuanceSheet);
		}
		
		private void UpdateIssuanceSheet(CollectiveExpense collectiveExpenseDoc, IUnitOfWork uow) {

			IssuanceSheet issuanceSheet = GetIssuanceSheet(collectiveExpenseDoc, uow);
			if(issuanceSheet?.CollectiveExpense == null)
				return;
			var issuanceSheetItems = issuanceSheet.Items.ToList();
			progressBar.Add(text: $"Отвязываем ведомость №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()} " +
			                      $"от документа коллективной выдачи №{issuanceSheet.CollectiveExpense.DocNumber ?? issuanceSheet.CollectiveExpense.Id.ToString()}");
			collectiveExpenseDoc.Comment += $" Ведомость №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()} отвязана от документа коллективной выдачи №" +
			                                $"{issuanceSheet.CollectiveExpense.DocNumber ?? issuanceSheet.CollectiveExpense.Id.ToString()}";
			if(issuanceSheetItems.IsNotEmpty())
				progressBar.Add(text: $"Очищаем ведомость №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()} " +
				                      "от операций выдачи сотрудникам");
			foreach(var item in issuanceSheetItems) {
				item.CollectiveExpenseItem = null;
				item.IssueOperation = null;
				uow.Save(item);
			}
			progressBar.Add(text: "Очищаем ссылку на документ коллективной выдачи №" +
			                      $"{issuanceSheet.CollectiveExpense.DocNumber ?? issuanceSheet.CollectiveExpense.Id.ToString()} " +
			                      $"в ведомости №{issuanceSheet.DocNumber ?? issuanceSheet.Id.ToString()}");
			issuanceSheet.CollectiveExpense = null;
			uow.Save(collectiveExpenseDoc);
			uow.Save(issuanceSheet);
		}
		#endregion

		#region Документы списания
		private IList<WriteoffItem> GetWriteOffItems(int[] employeeIssueOperationsIds, IUnitOfWork uow) {
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

		private void UpdateWriteWriteOffDocs
			(int[] employeeIssueOperationsIds,
			Dictionary<int, DutyNormIssueOperation> dutyNormWriteOffOperationByEmployeeWriteOffOperation,
			DutyNorm newDutyNorm,
			IUnitOfWork uow) {
			var writeOffItems = GetWriteOffItems(employeeIssueOperationsIds, uow);
			if(writeOffItems.IsNotEmpty())
				progressBar.Add(text: $"Перенос ссылок на операции списания по дежурной норме {newDutyNorm.Name} в документах списания");
			foreach(var item in writeOffItems) 
			{
				var dutyNormIssueOperation = dutyNormWriteOffOperationByEmployeeWriteOffOperation[item.EmployeeWriteoffOperation.Id];
				var removingWriteOffOperation = item.EmployeeWriteoffOperation;
				item.DutyNormWriteOffOperation = dutyNormIssueOperation;
				item.EmployeeWriteoffOperation = null;
				uow.Save(item);
				uow.Delete(removingWriteOffOperation);
			}
		}
		#endregion

		#region Операции со штрихкодами
		private void UpdateBarcodeOperations(
			Dictionary<int, DutyNormIssueOperation> dutyNormIssueOperationByIssueOperation, 
			IUnitOfWork uow) {
			var employeeIssueOperationsIds = dutyNormIssueOperationByIssueOperation.Keys.ToArray();
			var barcodeOperations = uow.Session.Query<BarcodeOperation>()
				.Where(x => employeeIssueOperationsIds.Contains(x.EmployeeIssueOperation.Id))
				.ToList();
			if(barcodeOperations.IsEmpty())
				return;
			foreach(var op in barcodeOperations) {
				progressBar.Add(text: $"Изменение ссылки операции выдачи в операции со штрихкодом {op.Barcode.Title}");
				op.DutyNormIssueOperation = dutyNormIssueOperationByIssueOperation[op.EmployeeIssueOperation.Id];
				op.EmployeeIssueOperation = null;
				uow.Save(op);
			}
		}
		#endregion

		#region Удаление
		private void RemoveNorm(Norm norm, IUnitOfWork uow) {
			if (norm.Employees.Any()) {
				progressBar.Add(text: "Очищение подвязанных сотрудников к норме");
				norm.Employees.Clear();
			}

			if (norm.Posts.Any()) {
				progressBar.Add(text: "Очищение подвязанных должностей к норме");
				norm.Posts.Clear();
			}

			if (norm.Items.Any()) {
				progressBar.Add(text: "Очищение строк нормы");
				norm.Items.Clear();
			}
			progressBar.Add(text: "Удаление нормы");
			uow.Delete(norm);
		}
		#endregion
	}
}
