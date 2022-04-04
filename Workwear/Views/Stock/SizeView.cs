using QS.Views.Dialog;
using workwear.Domain.Sizes;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class SizeView : EntityDialogViewBase<SizeViewModel, Size>
	{
		public SizeView(SizeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			
		}
	}
}
