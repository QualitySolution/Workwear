using System;
using QS.Views.Dialog;
using workwear.ViewModels.Tools;

namespace workwear.Views.Tools
{
	public partial class SendMessangeView : DialogViewBase<SendMessangeViewModel>
	{
		public SendMessangeView(SendMessangeViewModel viewModel): base(viewModel)
		{
			this.Build();
		}
	}
}
