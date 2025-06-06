using System;
using Gamma.Utilities;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Models.Operations
{
	public class OperationToDocumentReference
	{
		public int OperationId;
		public int? ExpenseId;
		public int? ExpenseItemId;
		public int? ExpenseDutyNormId;
		public int? ExpenseDutyNormItemId;
		public int? IncomeId;
		public int? ReturnId;
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

		public string ExpenseDocNumber;
		public string ExpenseDutyNormDocNumber;
		public string IncomeDocNumber;
		public string ReturnDocNumber;
		public string CollectiveExpenseDocNumber;
		public string TransferDocNumber;
		public string WriteoffDocNumber;
		public string InspectionDocNumber;
		public string CompletionFromResultDocNumber;
		public string CompletionFromSourceDocNumber;
		public string CompletionDocNumber => CompletionFromResultDocNumber ?? CompletionFromSourceDocNumber;
		
		public StockDocumentType? DocumentType {
			get {
				if(ExpenseId.HasValue)
					return StockDocumentType.ExpenseEmployeeDoc;
				if(ExpenseDutyNormId.HasValue)
					return StockDocumentType.ExpenseDutyNormDoc;
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
			ExpenseId ?? ExpenseDutyNormId ?? CollectiveExpenseId ?? IncomeId ?? ReturnId ?? TransferId ?? WriteoffId ?? CompletionId ?? InspectionId;
		public int? ItemId =>
			ExpenseItemId ?? ExpenseDutyNormItemId ?? CollectiveExpenseItemId ?? IncomeItemId ?? ReturnItemId ?? TransferItemId ?? WriteoffItemId ?? CompletionSourceItemId ?? CompletionResultItemId ?? InspectionItemId;
		public string DocumentNumber =>
			ExpenseDocNumber ?? ExpenseDutyNormDocNumber ?? CollectiveExpenseDocNumber ?? IncomeDocNumber ?? ReturnDocNumber ?? TransferDocNumber ?? WriteoffDocNumber ?? CompletionDocNumber ?? InspectionDocNumber;
		public string DocumentTitle => $"{DocumentType?.GetEnumTitle()} №{(String.IsNullOrWhiteSpace(DocumentNumber) ? DocumentId.ToString() : DocumentNumber)}";
	}
}
