using System;
using QS.Views;
using workwear.Journal.Filter.ViewModels.Regulations;

namespace workwear.Journal.Filter.Views.Regulations
{
	public partial class NormFilterView : ViewBase<NormFilterViewModel>
	{
		public NormFilterView(NormFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			entryPost.ViewModel = viewModel.EntryPost;
			entryProtectionTools.ViewModel = viewModel.EntryProtectionTools;
		}
	}
}
