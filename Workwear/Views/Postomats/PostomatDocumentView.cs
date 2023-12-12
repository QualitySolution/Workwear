using System;
using System.Linq;
using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QS.Cloud.Postomat.Manage;
using QS.Views.Dialog;
using Workwear.Domain.Postomats;
using Workwear.ViewModels.Postomats;

namespace Workwear.Views.Postomats {

	public partial class PostomatDocumentView : EntityDialogViewBase<PostomatDocumentViewModel, PostomatDocument> {

		public PostomatDocumentView(PostomatDocumentViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {
			labelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();
			labelStatus.Binding.AddFuncBinding(Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			comboTypeDoc.ItemsEnum = typeof(DocumentType);
			comboTypeDoc.Binding.AddBinding(Entity, e => e.Type, w => w.SelectedItem).InitializeFromSource();
			ydateDoc.Binding.AddBinding(Entity, e => e.CreateTime, w => w.Date).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			comboPostomat.SetRenderTextFunc<PostomatInfo>(p => $"{p.Id} {p.Name}({p.Location})");
			comboPostomat.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Postomats, w => w.ItemsList)
				.AddBinding(v => v.Postomat, w => w.SelectedItem)
				.InitializeFromSource();

			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<PostomatDocumentItem>()
				.AddColumn("Наименование").AddReadOnlyTextRenderer(x => x.Nomenclature?.Name)
				.AddColumn("Штрихкод").AddReadOnlyTextRenderer(x => x.Barcode?.Title)
				.AddColumn("Количество").AddNumericRenderer(x => x.Delta)
				.AddColumn("Место хранения").AddComboRenderer(x => x.Location)
					.SetDisplayFunc(x => x.Title)
					.DynamicFillListFunc(x => ViewModel.AvailableCellsFor(x).ToList())
					.Editing()
				.Finish();
			treeItems.Binding.AddBinding(Entity, e => e.Items, w => w.ItemsDataSource).InitializeFromSource();
			treeItems.Selection.Changed += SelectionOnChanged;
			
			SetEditableWindow();
			
			buttonDel.Clicked += (sender, args) => ViewModel.RemoveItem(treeItems.GetSelectedObject<PostomatDocumentItem>());
		}

		private void SelectionOnChanged(object sender, EventArgs e) {
			buttonDel.Sensitive = treeItems.Selection.CountSelectedRows() > 0;
		}

		private void SetEditableWindow() {
			comboPostomat.Sensitive = 
				buttonAdd.Sensitive = 
					comboTypeDoc.Sensitive = 
						ytextComment.Sensitive = 
							treeItems.Sensitive = 
								ydateDoc.Sensitive = ViewModel.CanEdit;
		}
	}
}
