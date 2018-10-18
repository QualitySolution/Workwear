using System;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using QSTDI;
using workwear.Domain.Stock;
using workwear.Representations;

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
			var dlg = OrmMain.CreateObjectDialog(StockDocument.GetDocClass(type));
			TabParent.AddTab(dlg, this);
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
							OrmMain.GenerateDialogHashName<Income>(node.Id),
							() => new IncomeDocDlg(node.Id),
							this);
						break;
					case StokDocumentType.ExpenseDoc:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<Expense>(node.Id),
							() => new ExpenseDocDlg(node.Id),
							this);
						break;
					case StokDocumentType.WriteoffDoc:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<Writeoff>(node.Id),
							() => new WriteOffDocDlg(node.Id),
							this);
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
