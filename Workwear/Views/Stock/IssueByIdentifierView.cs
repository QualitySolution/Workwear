using System;
using QS.Views.Dialog;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class IssueByIdentifierView : DialogViewBase<IssueByIdentifierViewModel>
	{
		public IssueByIdentifierView(IssueByIdentifierViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ylabelCardID.Binding.AddBinding(ViewModel, v => v.CardID, w => w.LabelProp).InitializeFromSource();
		}
	}
}
