using System;
using QS.Views.Dialog;
using Workwear.ViewModels.IdentityCards;

namespace Workwear.Views.IdentityCards
{
	public partial class ReadCardView : DialogViewBase<ReadCardViewModel>
	{
		public ReadCardView(ReadCardViewModel viewModel) : base(viewModel)
		{
			this.Build();

			buttonSave.Binding.AddBinding(ViewModel, v => v.SensetiveSaveButton, w => w.Sensitive).InitializeFromSource();
			labelStatus.Binding.AddFuncBinding(ViewModel, v => $"<span foreground=\"{v.StatusColor}\">{v.Status}</span>" , w => w.LabelProp).InitializeFromSource();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			ViewModel.Apply();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, QS.Navigation.CloseSource.Cancel);
		}
	}
}
