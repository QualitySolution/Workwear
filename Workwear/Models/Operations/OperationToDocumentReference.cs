using Gamma.Utilities;
using workwear.Domain.Stock;

namespace workwear.Models.Operations
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
		public int? MassExpenseId;
		public int? MassOperationItemItemId;

		public StokDocumentType? DocumentType {
			get {
				if(ExpenceId.HasValue)
					return ExpenseOperation == ExpenseOperations.Employee ? StokDocumentType.ExpenseEmployeeDoc : StokDocumentType.ExpenseObjectDoc;
				if(CollectiveExpenseId.HasValue)
					return StokDocumentType.CollectiveExpense;
				if(IncomeId.HasValue)
					return StokDocumentType.IncomeDoc;
				if(TransferId.HasValue)
					return StokDocumentType.TransferDoc;
				if(WriteoffId.HasValue)
					return StokDocumentType.WriteoffDoc;

				return null;
			}
		}
		//Внимание здесь последовательность получения ID желательно сохранять такую же как у типа документа.
		//Так как в случае ошибочной связи операции с двумя документами возьмется первый найденный в обоих случаях, иначе будет тип одного, а id от другого.
		public int? DocumentId => ExpenceId ?? CollectiveExpenseId ?? IncomeId ?? TransferId ?? WriteoffId;

		public int? ItemId => ExpenceItemId ?? CollectiveExpenseItemId ?? IncomeItemId ?? TransferItemId ?? WriteoffItemId;

		public string DocumentTitle => $"{DocumentType?.GetEnumTitle()} №{DocumentId}";
	}

	public enum OperationType
	{
		EmployeeIssue,
		Warehouse
	}
}
