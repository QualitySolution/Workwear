using QS.Views.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock 
{
	public partial class BarcodeView : EntityDialogViewBase<BarcodeViewModel, Barcode> 
	{
		public BarcodeView(BarcodeViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			CommonButtonSubscription();
			
			ylabelCodeValue.Binding
				.AddFuncBinding(Entity, e => e.Id.ToString(), w => w.LabelProp).InitializeFromSource();
			labelNomenclature.Binding
				.AddFuncBinding(Entity, e => e.Nomenclature.Name, w => w.LabelProp).InitializeFromSource();
			labelTitle.Binding
				.AddFuncBinding(Entity, e => e.Title, w => w.LabelProp).InitializeFromSource();
			labelCreateDate.Binding
				.AddFuncBinding(Entity, e => e.CreateDate.ToShortDateString(), w => w.LabelProp).InitializeFromSource();
			labelHeight.Binding
				.AddFuncBinding(Entity, e => e.Height != null ? e.Height.Name : null, w => w.LabelProp).InitializeFromSource();
			labelSize.Binding
				.AddFuncBinding(Entity, e => e.Size != null ? e.Size.Name : null, w => w.LabelProp).InitializeFromSource();
			treeviewOperations.CreateFluentColumnsConfig<BarcodeOperation>()
				.AddColumn("Дата").AddTextRenderer(x => $"{x.OperationDate:d}")
				.AddColumn("Операция").AddTextRenderer(x => x.OperationTitle)
				.Finish();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			buttonPrintBarcode.Binding
				.AddBinding(ViewModel, vm => vm.CanPrint, w => w.Visible).InitializeFromSource();
			buttonPrintBarcode.Clicked += (sender, args) => ViewModel.PrintBarcodes();
			
			treeviewOperations.ItemsDataSource = Entity.BarcodeOperations;
		}
	}
}
