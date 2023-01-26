using System;
using QS.Views.Dialog;
using Workwear.ViewModels.Stock.Widgets;

using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using Workwear.Domain.Sizes;

namespace Workwear.Views.Stock.Widgets {
	[System.ComponentModel.ToolboxItem(true)] //?
	
	public partial class IssueWidgetView : DialogViewBase<IssueWidgetViewModel> {
		public IssueWidgetView(IssueWidgetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {
		}

	}
}
