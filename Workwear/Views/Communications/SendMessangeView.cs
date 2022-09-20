using QS.Navigation;
using QS.Views.Dialog;
using Workwear.Domain.Communications;
using Workwear.ViewModels.Communications;

namespace Workwear.Views.Communications
{
	public partial class SendMessangeView : DialogViewBase<SendMessangeViewModel>
	{
		public SendMessangeView(SendMessangeViewModel viewModel): base(viewModel)
		{
			this.Build();

			ylistcomboboxTemplates.SetRenderTextFunc<MessageTemplate>(x => x.Name);
			ylistcomboboxTemplates.Binding.AddSource(viewModel)
				.AddBinding(v => v.Templates, w => w.ItemsList)
				.AddBinding(v => v.SelectedTemplate, w => w.SelectedItem)
				.InitializeFromSource();
			yentryTitle.Binding.AddBinding(viewModel, v => v.MessageTitle, w => w.Text).InitializeFromSource();
			ytextTemplate.Binding.AddBinding(viewModel, v => v.MessageText, w => w.Buffer.Text).InitializeFromSource();
			ybuttonSend.Binding.AddBinding(viewModel, v => v.SensitiveSendButton, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonCancel(object sender, System.EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Self);
		}

		protected void OnButtonSend(object sender, System.EventArgs e)
		{
			ViewModel.SendMessage();
		}
	}
}
