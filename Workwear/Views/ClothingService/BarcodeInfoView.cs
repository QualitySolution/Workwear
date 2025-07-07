using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BarcodeInfoView : Gtk.Bin {
		public BarcodeInfoView() {
			this.Build();
		}
		
		private BarcodeInfoViewModel viewModel;
		public BarcodeInfoViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				labelEmployee.Binding
					.AddFuncBinding(ViewModel, e => e.LabelEmployee, w => w.LabelProp)
					.InitializeFromSource();

				labelNomenclature.Binding
					.AddFuncBinding(ViewModel, e => e.LabelNomenclature, w => w.LabelProp)
					.InitializeFromSource();

				labelTitle.Binding
					.AddFuncBinding(ViewModel, e => e.LabelTitle, w => w.LabelProp)
					.InitializeFromSource();

				labelCreateDate.Binding
					.AddFuncBinding(ViewModel, e => e.LabelCreateDate, w => w.LabelProp)
					.InitializeFromSource();

				labelHeight.Binding
					.AddFuncBinding(ViewModel, e => e.LabelHeight, w => w.LabelProp)
					.InitializeFromSource();

				labelSize.Binding
					.AddFuncBinding(ViewModel, e => e.LabelSize, w => w.LabelProp)
					.InitializeFromSource();

				tableInfo.Binding
					.AddBinding(ViewModel, v => v.VisibleBarcode, w => w.Visible)
					.InitializeFromSource();

				labelInfo.Binding
					.AddSource(ViewModel)
					.AddFuncBinding(v => v.LabelInfo, w => w.LabelProp)
					.AddBinding(v => v.VisibleInfo, w => w.Visible)
					.InitializeFromSource();
			}
		}
	}
}
