using System;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.Models.Operations;
using Workwear.ViewModels.Stock;

namespace workwear.Models.Stock
{
	public class OpenStockDocumentsModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private ITdiCompatibilityNavigation navigation;

		public OpenStockDocumentsModel(ITdiCompatibilityNavigation navigation)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		public void CreateDocumentDialog(DialogViewModelBase master, StockDocumentType documentType)
		{
			switch (documentType) {
				case StockDocumentType.TransferDoc:
					navigation.OpenViewModel<WarehouseTransferViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.CollectiveExpense:
					navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.ExpenseEmployeeDoc:
					navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.ExpenseDutyNormDoc:
					navigation.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.Return:
					navigation.OpenViewModel<ReturnViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.Income:
					navigation.OpenViewModel<IncomeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.WriteoffDoc:
					navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.Completion:
					navigation.OpenViewModel<CompletionViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.InspectionDoc:
					navigation.OpenViewModel<InspectionViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.OverNormDoc:
					navigation.OpenViewModel<OverNormViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StockDocumentType.BarcodingDoc:
					navigation.OpenViewModel<BarcodingViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				default:
					throw new NotSupportedException($"Тип документа {documentType} не поддерживается.");
			}
		}

		public virtual void EditDocumentDialog(DialogViewModelBase master, OperationToDocumentReference reference)
		{
			var page = EditDocumentDialog(master, reference.DocumentType.Value, reference.DocumentId.Value);
			if(page.ViewModel is ISelectItem select && reference.ItemId.HasValue)
				select.SelectItem(reference.ItemId.Value);
		}

		public virtual IPage EditDocumentDialog(DialogViewModelBase master, StockDocumentType documentType, int id)
		{
			var start = DateTime.Now;
			var page = OpenEditDialog(master, documentType, id);
			logger.Info($"Документ открыт за {(DateTime.Now - start).TotalSeconds} сек.");
			return page;
		}

		private IPage OpenEditDialog(DialogViewModelBase master, StockDocumentType documentType, int id)
		{
			switch (documentType)
			{
				case StockDocumentType.Income:
					return navigation.OpenViewModel<IncomeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.Return:
					return navigation.OpenViewModel<ReturnViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.ExpenseEmployeeDoc:
					return navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.ExpenseDutyNormDoc:
					return navigation.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.CollectiveExpense:
					return navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.WriteoffDoc:
					return navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.TransferDoc:
					return navigation.OpenViewModel<WarehouseTransferViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.Completion:
					return navigation.OpenViewModel<CompletionViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.InspectionDoc:
					return navigation.OpenViewModel<InspectionViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.OverNormDoc:
					return navigation.OpenViewModel<OverNormViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StockDocumentType.BarcodingDoc:
					return navigation.OpenViewModel<BarcodingViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				default:
					throw new NotSupportedException($"Тип документа {documentType} не поддерживается.");
			}
		}
	}
}
