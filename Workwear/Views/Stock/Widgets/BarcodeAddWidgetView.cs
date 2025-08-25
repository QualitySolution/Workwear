using QS.Views;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock.Widgets ;

namespace Workwear.Views.Stock.Widgets {
	public partial class BarcodeAddWidgetView  : ViewBase<BarcodeAddWidgetViewModel> {
		public BarcodeAddWidgetView(BarcodeAddWidgetViewModel viewModel) : base(viewModel) {
			this.Build();

			yhboxEntryCode.Binding
				.AddBinding(ViewModel, vm => vm.CanEntry, w => w.Sensitive).InitializeFromSource();
			entryCode.Binding
				.AddBinding(ViewModel, vm => vm.BarcodeText, w => w.Text).InitializeFromSource();
			ycheckbuttonAutoAdd.Binding
				.AddBinding(ViewModel, vm => vm.AutoAdd, w => w.Active).InitializeFromSource();
			buttonAdd.Binding
				.AddBinding(ViewModel, vm => vm.CanAdd, w => w.Sensitive).InitializeFromSource();
			ylabelCode.Binding
				.AddBinding(ViewModel, vm => vm.CodeLabel, w => w.LabelProp).InitializeFromSource();
			ylabelCheck.Binding
				.AddBinding(ViewModel, vm => vm.CheckText, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.CheckTextColor,w => w.ForegroundColor)
				.InitializeFromSource();                                                                         			

			buttonAdd.Clicked += (sender, args) => ViewModel.AddItem();
			buttonAccept.Clicked += (sender, args) => ViewModel.Accept();
			
			treeAdded.Binding
				.AddBinding(ViewModel, vm => vm.AddedBarcodes, w => w.ItemsDataSource)
				.InitializeFromSource();
			treeAdded.CreateFluentColumnsConfig<Barcode>()
				.AddColumn("Добавить").AddReadOnlyTextRenderer(x => x.Title)
				.Finish();
            
		}
	}
}
