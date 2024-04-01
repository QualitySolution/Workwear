using System;
using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Workwear.Domain.Postomats;
using Workwear.ViewModels.Postomats;

namespace Workwear.Views.Postomats 
{
	public partial class PostomatDocumentWithdrawView : EntityDialogViewBase<PostomatDocumentWithdrawViewModel, PostomatDocumentWithdraw>  
	{
		public PostomatDocumentWithdrawView(PostomatDocumentWithdrawViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			
			CommonButtonSubscription();
			ConfigureDlg();
		}
		
		private void ConfigureDlg() 
		{
			labelId.Binding.AddBinding(Entity, p => p.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();
			ydateDoc.Binding.AddBinding(Entity, p => p.CreateTime, w => w.Date).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, p => p.Comment, w => w.Buffer.Text).InitializeFromSource();
			
			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<PostomatDocumentWithdrawItem>()
				.AddColumn("Сотрудник").AddReadOnlyTextRenderer(x => x.Employee?.ShortName)
				.AddColumn("Наименование").AddReadOnlyTextRenderer(x => x.Nomenclature?.Name)
				.AddColumn("Штрихкод").AddReadOnlyTextRenderer(x => x.Barcode?.Title)
				.AddColumn("Постамат").AddReadOnlyTextRenderer(x => x.TerminalLocation)
				.Finish();
			treeItems.Binding.AddBinding(Entity, p => p.Items, w => w.ItemsDataSource).InitializeFromSource();
			
			treeItems.Selection.Changed += SelectionOnChanged;

			SetEditableElements();
		}

		private void SetEditableElements()
		{
			deleteButton.Visible = treeItems.Sensitive = fillDataButton.Visible = ViewModel.CanEdit;
		}
		
		private void SelectionOnChanged(object sender, EventArgs e) 
		{
			deleteButton.Sensitive = treeItems.Selection.CountSelectedRows() > 0;
		}
		
		private void OnDeleteButtonClicked(object sender, EventArgs e) 
		{
			ViewModel.RemoveItem(treeItems.GetSelectedObject<PostomatDocumentWithdrawItem>());
		}
		
		private void OnFillDataButtonClicked(object sender, EventArgs e) 
		{
			ViewModel.TryFillData();
		}

		protected void OnPrintButtonClicked(object sender, EventArgs e) 
		{
			ViewModel.Print();
		}
	}
}
