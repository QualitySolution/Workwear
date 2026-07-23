using System;
using QS.Views.Dialog;
using Workwear.Domain.Sizes;
using Workwear.ViewModels.Sizes;

namespace Workwear.Views.Sizes
{
	public partial class StockPositionSizeReplaceView : DialogViewBase<StockPositionSizeReplaceViewModel>
	{
		public StockPositionSizeReplaceView(StockPositionSizeReplaceViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			// Номенклатура
			ylabelNomenclatureName.Binding
				.AddBinding(ViewModel, vm => vm.NomenclatureName, w => w.LabelProp)
				.InitializeFromSource();

			// Размер
			yhboxSize.Binding
				.AddBinding(ViewModel, vm => vm.VisibleSize, w => w.Visible)
				.InitializeFromSource();
			ylabelCurrentSize.Binding
				.AddBinding(ViewModel, vm => vm.CurrentSizeName, w => w.LabelProp)
				.InitializeFromSource();
			ycomboNewSize.SetRenderTextFunc<Size>(s => s.Name);
			ycomboNewSize.ItemsList = ViewModel.AvailableSizes;
			ycomboNewSize.Binding
				.AddBinding(ViewModel, vm => vm.NewSize, w => w.SelectedItem)
				.InitializeFromSource();

			// Рост
			yhboxHeight.Binding
				.AddBinding(ViewModel, vm => vm.VisibleHeight, w => w.Visible)
				.InitializeFromSource();
			ylabelCurrentHeight.Binding
				.AddBinding(ViewModel, vm => vm.CurrentHeightName, w => w.LabelProp)
				.InitializeFromSource();
			ycomboNewHeight.SetRenderTextFunc<Size>(s => s.Name);
			ycomboNewHeight.ItemsList = ViewModel.AvailableHeights;
			ycomboNewHeight.Binding
				.AddBinding(ViewModel, vm => vm.NewHeight, w => w.SelectedItem)
				.InitializeFromSource();

			// Кнопки
			ybuttonReplace.Binding
				.AddBinding(ViewModel, vm => vm.CanReplace, w => w.Sensitive)
				.InitializeFromSource();
		}

		protected void OnButtonReplaceClicked(object sender, EventArgs e)
		{
			ViewModel.Replace();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Cancel();
		}
	}
}

