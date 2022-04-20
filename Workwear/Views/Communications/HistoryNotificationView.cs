using System;
using QS.Views;
using workwear.ViewModels.Communications;

namespace workwear.Views.Communications
{
	public partial class HistoryNotificationView : ViewBase<HistoryNotificationViewModel> 
	{
		public HistoryNotificationView(HistoryNotificationViewModel viewModel): base(viewModel)
			{
				this.Build();
			}
	}
}
