using Gamma.Utilities;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Models.Operations
{
	public class OperationToDocumentReference
	{
		public int OperationId;
		//Пока добавлено на всякий случай
		public OperationType OperationType;
		public int? ExpenceId;
		public int? ExpenceItemId;
		public ExpenseOperations? ExpenseOperation;
		public int? IncomeId;
		public int? IncomeItemId;
		public int? CollectiveExpenseId;
		public int? CollectiveExpenseItemId;
		public int? TransferId;
		public int? TransferItemId;
		public int? WriteoffId;
		public int? WriteoffItemId;
		public int? InspectionId;
		public int? InspectionItemId;
		public int? CompletionFromSourceId;
		public int? CompletionFromResultId;
		public int? CompletionSourceItemId;
		public int? CompletionResultItemId;
		public int? CompletionId => CompletionFromSourceId ?? CompletionFromResultId;

		public StockDocumentType? DocumentType {
			get {
				if(ExpenceId.HasValue)
					return ExpenseOperation == ExpenseOperations.Employee ? StockDocumentType.ExpenseEmployeeDoc : StockDocumentType.ExpenseObjectDoc;
				if(CollectiveExpenseId.HasValue)
					return StockDocumentType.CollectiveExpense;
				if(IncomeId.HasValue)
					return StockDocumentType.IncomeDoc;
				if(TransferId.HasValue)
					return StockDocumentType.TransferDoc;
				if(WriteoffId.HasValue)
					return StockDocumentType.WriteoffDoc;
				if (CompletionId.HasValue)
					return StockDocumentType.Completion;
				if(InspectionId.HasValue)
					return StockDocumentType.InspectionDoc;

				return null;
			}
		}
		//Внимание здесь последовательность получения ID желательно сохранять такую же как у типа документа.
		//Так как в случае ошибочной связи операции с двумя документами возьмется первый найденный в обоих случаях, иначе будет тип одного, а id от другого.
		public int? DocumentId =>
			ExpenceId ?? CollectiveExpenseId ?? IncomeId ?? TransferId ?? WriteoffId ?? CompletionId ?? InspectionId;

		public int? ItemId => ExpenceItemId ?? CollectiveExpenseItemId ??
			IncomeItemId ?? TransferItemId ?? WriteoffItemId ?? CompletionSourceItemId ?? CompletionResultItemId ?? InspectionItemId;

		public string DocumentTitle => $"{DocumentType?.GetEnumTitle()} №{DocumentId}";
	}

	public enum OperationType
	{
		EmployeeIssue,
		Warehouse
	}
}
