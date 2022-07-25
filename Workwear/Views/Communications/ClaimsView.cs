using QS.Views.Dialog;
using workwear.ViewModels.Communications;

namespace workwear.Views.Communications 
{
	public partial class ClaimsView : DialogViewBase<ClaimsViewModel> 
	{
		public ClaimsView(ClaimsViewModel viewModel) : base(viewModel) 
		{
			this.Build();
		}
	}
}
