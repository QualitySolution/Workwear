using System;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Repository.Operations;
using workwear.ViewModels.Stock;

namespace workwear.Models.Stock
{
	public class OpenStockDocumentsModel
	{
		private ITdiCompatibilityNavigation navigation;

		public OpenStockDocumentsModel(ITdiCompatibilityNavigation navigation)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		public void CreateDocumentDialog(DialogViewModelBase master, StokDocumentType documentType)
		{
			if(documentType == StokDocumentType.TransferDoc) 
				navigation.OpenViewModel<WarehouseTransferViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
			else if(documentType == StokDocumentType.MassExpense)
				navigation.OpenViewModel<MassExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
			else if(documentType == StokDocumentType.CollectiveExpense)
				navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
			else if(documentType == StokDocumentType.ExpenseEmployeeDoc)
				navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
			else if(documentType == StokDocumentType.ExpenseObjectDoc)
				navigation.OpenViewModel<ExpenseObjectViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForCreate());
			else if(documentType == StokDocumentType.IncomeDoc)
				navigation.OpenTdiTab<IncomeDocDlg>(master);
			else if (documentType == StokDocumentType.WriteoffDoc)
				navigation.OpenTdiTab<WriteOffDocDlg>(master);
			else
				throw new NotSupportedException($"Тип документа {documentType} не поддерживается.");
		}

		public virtual void EditDocumentDialog(DialogViewModelBase master, EmployeeIssueReference reference)
		{
			EditDocumentDialog(master, reference.DocumentType.Value, reference.DocumentId.Value);
		}
		public virtual void EditDocumentDialog(DialogViewModelBase master, StokDocumentType documentType, int id)
		{
			switch (documentType)
			{
				case StokDocumentType.IncomeDoc:
					navigation.OpenTdiTab<IncomeDocDlg, int>(master, id);
					break;
				case StokDocumentType.ExpenseEmployeeDoc:
					navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
					break;
				case StokDocumentType.ExpenseObjectDoc:
					navigation.OpenViewModel<ExpenseObjectViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
					break;
				case StokDocumentType.CollectiveExpense:
					navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
					break;
				case StokDocumentType.WriteoffDoc:
					navigation.OpenTdiTab<WriteOffDocDlg, int>(master, id);
					break;
				case StokDocumentType.TransferDoc:
					navigation.OpenViewModel<WarehouseTransferViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
					break;
				case StokDocumentType.MassExpense:
					navigation.OpenViewModel<MassExpenseViewModel, IEntityUoWBuilder>(master, EntityUoWBuilder.ForOpen(id));
					break;
				default:
					throw new NotSupportedException($"Тип документа {documentType} не поддерживается.");
			}
		}
	}
}