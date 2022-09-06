using QS.Views.Dialog;
using workwear.ViewModels.Communications;

namespace workwear.Views.Communications 
{
	public partial class RatingsView : DialogViewBase<RatingsViewModel>
	{
		public RatingsView(RatingsViewModel viewModel) : base(viewModel) 
		{
			this.Build();

			entityNomenclature.ViewModel = viewModel.EntryNomenclature;
		}
	}
}
