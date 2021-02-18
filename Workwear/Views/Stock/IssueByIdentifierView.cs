using System;
using QS.Views.Dialog;
using workwear.Tools.IdentityCards;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class IssueByIdentifierView : DialogViewBase<IssueByIdentifierViewModel>
	{
		public IssueByIdentifierView(IssueByIdentifierViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ylabelCardID.Binding.AddBinding(ViewModel, v => v.CardID, w => w.LabelProp).InitializeFromSource();
			comboDevice.SetRenderTextFunc<DeviceInfo>(x => x.Title);
			comboDevice.Binding.AddSource(viewModel)
				.AddBinding(v => v.Devices, w => w.ItemsList)
				.AddBinding(v => v.SelectedDevice, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}
