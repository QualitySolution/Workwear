using QS.Views.Dialog;
using workwear.Domain.Sizes;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class SizeTypeView : EntityDialogViewBase<SizeTypeViewModel, SizeType>
	{
		public SizeTypeView(SizeTypeViewModel viewModel) : base(viewModel)
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
