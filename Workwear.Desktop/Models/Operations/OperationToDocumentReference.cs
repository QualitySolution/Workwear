using System;
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
		public int? IncomeId;
		public int? ReturnId;
//public IncomeOperations? IncomeOperation;
		public int? IncomeItemId;
		public int? ReturnItemId;
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

		public string IncomeDocNumber;
		public string ReturnDocNumber;

		public StockDocumentType? DocumentType {
			get {
				if(ExpenceId.HasValue)
					return StockDocumentType.ExpenseEmployeeDoc;
				if(CollectiveExpenseId.HasValue)
					return StockDocumentType.CollectiveExpense;
				if(IncomeId.HasValue) 
					return StockDocumentType.Income;
				if(ReturnId.HasValue) 
					return StockDocumentType.Return;
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
			ExpenceId ?? CollectiveExpenseId ?? IncomeId ?? ReturnId ?? TransferId ?? WriteoffId ?? CompletionId ?? InspectionId;

		public int? ItemId => ExpenceItemId ?? CollectiveExpenseItemId ??
			IncomeItemId ?? ReturnItemId ?? TransferItemId ?? WriteoffItemId ?? CompletionSourceItemId ?? CompletionResultItemId ?? InspectionItemId;

		public string DocumentNumber => IncomeDocNumber ?? ReturnDocNumber;
		
		public string DocumentTitle => $"{DocumentType?.GetEnumTitle()} №{(String.IsNullOrWhiteSpace(DocumentNumber) ? DocumentId.ToString() : DocumentNumber)}";
	}

	public enum OperationType
	{
		EmployeeIssue,
		Warehouse
	}
}
