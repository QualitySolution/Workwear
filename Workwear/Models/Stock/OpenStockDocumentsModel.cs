using System;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Models.Operations;
using workwear.ViewModels.Stock;

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

		public void CreateDocumentDialog(DialogViewModelBase master, StokDocumentType documentType)
		{
			switch (documentType) {
				case StokDocumentType.TransferDoc:
					navigation.OpenViewModel<WarehouseTransferViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StokDocumentType.CollectiveExpense:
					navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StokDocumentType.ExpenseEmployeeDoc:
					navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StokDocumentType.ExpenseObjectDoc:
					navigation.OpenViewModel<ExpenseObjectViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StokDocumentType.IncomeDoc:
					navigation.OpenTdiTab<IncomeDocDlg>(master);
					break;
				case StokDocumentType.WriteoffDoc:
					navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
					break;
				case StokDocumentType.Completion:
					navigation.OpenViewModel<CompletionViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
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

		public virtual IPage EditDocumentDialog(DialogViewModelBase master, StokDocumentType documentType, int id)
		{
			var start = DateTime.Now;
			var page = OpenEditDialog(master, documentType, id);
			logger.Info($"Документ открыт за {(DateTime.Now - start).TotalSeconds} сек.");
			return page;
		}

		private IPage OpenEditDialog(DialogViewModelBase master, StokDocumentType documentType, int id)
		{
			switch (documentType)
			{
				case StokDocumentType.IncomeDoc:
					return navigation.OpenTdiTab<IncomeDocDlg, int>(master, id);
				case StokDocumentType.ExpenseEmployeeDoc:
					return navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StokDocumentType.ExpenseObjectDoc:
					return navigation.OpenViewModel<ExpenseObjectViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StokDocumentType.CollectiveExpense:
					return navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StokDocumentType.WriteoffDoc:
					return navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StokDocumentType.TransferDoc:
					return navigation.OpenViewModel<WarehouseTransferViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				case StokDocumentType.Completion:
					return navigation.OpenViewModel<CompletionViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
				default:
					throw new NotSupportedException($"Тип документа {documentType} не поддерживается.");
			}
		}
	}
}