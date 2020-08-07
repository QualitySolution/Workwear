using System;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using workwear.Domain.Stock;
using workwear.Representations;
using workwear.ViewModels.Stock;

namespace workwear.JournalViewers
{
	public partial class StockDocumentsView : TdiTabBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		IUnitOfWork uow;

		public StockDocumentsView()
		{
			this.Build();
			this.TabName = "Журнал документов";
			tableDocuments.Selection.Changed += OnSelectionChanged;
			tableDocuments.RepresentationModel = new StockDocumentsVM();
			hboxFilter.Add(tableDocuments.RepresentationModel.RepresentationFilter as Widget);
			(tableDocuments.RepresentationModel.RepresentationFilter as Widget).Show();
			tableDocuments.RepresentationModel.UpdateNodes();
			uow = tableDocuments.RepresentationModel.UoW;

			buttonAdd.ItemsEnum = typeof(StokDocumentType);
		}

		void OnRefObjectUpdated(object sender, OrmObjectUpdatedEventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes();
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = tableDocuments.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			StokDocumentType type = (StokDocumentType)e.ItemEnum;
			if(type == StokDocumentType.TransferDoc) {
				MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<WarehouseTransferViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			}
			else if(type == StokDocumentType.MassExpense)
				MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<WarehouseMassExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			else if(type == StokDocumentType.ExpenseEmployeeDoc)
				MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<ExpenseEmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			else if(type == StokDocumentType.ExpenseObjectDoc)
				MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<ExpenseObjectViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			else {
				var dlg = OrmMain.CreateObjectDialog(StockDocument.GetDocClass(type));
				TabParent.AddTab(dlg, this);
			}
		}

		protected void OnTableDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			if (tableDocuments.GetSelectedObjects().GetLength(0) > 0)
			{
				var node = tableDocuments.GetSelectedObject<StockDocumentsVMNode>();

				switch (node.DocTypeEnum)
				{
					case StokDocumentType.IncomeDoc:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<Income>(node.Id),
							() => new IncomeDocDlg(node.Id),
							this);
						break;
					case StokDocumentType.ExpenseDoc:
						switch(node.ExpenseOperation) {
							case ExpenseOperations.Employee:
								MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<ExpenseEmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
								break;
							case ExpenseOperations.Object:
								MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<ExpenseObjectViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
								break;
							default:
								throw new NotImplementedException();
						}
						break;
					case StokDocumentType.WriteoffDoc:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<Writeoff>(node.Id),
							() => new WriteOffDocDlg(node.Id),
							this);
						break;
					case StokDocumentType.TransferDoc:
						MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<WarehouseTransferViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
						break;
					case StokDocumentType.MassExpense:
						MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<WarehouseMassExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
						break;
					default:
						throw new NotSupportedException("Тип документа не поддерживается.");
				}
			}
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var item = tableDocuments.GetSelectedObject<StockDocumentsVMNode>();
			if (OrmMain.DeleteObject(StockDocument.GetDocClass(item.DocTypeEnum), item.Id))
				tableDocuments.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonFilterToggled(object sender, EventArgs e)
		{
			hboxFilter.Visible = buttonFilter.Active;
		}

		protected void OnSearchentity1TextChanged(object sender, EventArgs e)
		{
			tableDocuments.SearchHighlightText = searchentity1.Text;
			tableDocuments.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes();
		}
	}
}
