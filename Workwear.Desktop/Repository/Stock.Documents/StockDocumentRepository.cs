using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Repository.Stock.Documents {
	
	public class StockDocumentRepository {
		#region Документы выдачи
		/// <summary>
		/// Получаем документы выдачи со строками, соответствующими выдачам по норме для текущего сотрудника.
		/// Т.е. если были выдачи сверх нормы в документе, мы вернем только те строки, где выдачи были по норме.
		/// </summary>
		/// <returns></returns>
		public Dictionary<Expense, List<ExpenseItem>> GetExpenseDocsForEmployee (int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			
			var expenseItems = uow.Session.QueryOver<ExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List();
			
			var expenseDocs = expenseItems
				.GroupBy(x => x.ExpenseDoc)
				.ToDictionary(g => g.Key, g => g.ToList());

			return expenseDocs;
		}
		
		public Dictionary<CollectiveExpense, List<CollectiveExpenseItem>> GetCollectiveExpenseDocsForEmployee(int[] employeeIssueOperationsIds, IUnitOfWork uow) {
			
			var collectiveExpenseItems = uow.Session.QueryOver<CollectiveExpenseItem>()
				.Where(x => x.EmployeeIssueOperation.Id.IsIn(employeeIssueOperationsIds))
				.List();
			var collectiveExpenseDocs = collectiveExpenseItems
				.GroupBy(x => x.Document)
				.ToDictionary(g => g.Key, g => g.ToList());

			return collectiveExpenseDocs;
		}
		#endregion
	}
}
