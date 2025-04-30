using QS.Views.Dialog;
using QS.Widgets;
using Workwear.ViewModels.Communications;

namespace Workwear.Views.Communications {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendEmailView  : DialogViewBase<SendEmailViewModel> {
		public SendEmailView(SendEmailViewModel viewModel): base(viewModel)
		{
			this.Build();
			
			yentryEAdress.ValidationMode = ValidationType.Email;
			yentryEAdress.Binding
				.AddBinding(ViewModel, vm => vm.EmailAddres, w => w.Text).InitializeFromSource();
			yentryTopic.Binding
            	.AddBinding(ViewModel, vm => vm.Topic, w => w.Text).InitializeFromSource();
            ytextMessage.Binding
				.AddBinding(ViewModel, vm => vm.Messege, w => w.Buffer.Text).InitializeFromSource();
			
			ybuttonSaveAdress.Clicked += (o,e) => ViewModel.SaveAdress();
			ybuttonSaveAdress.Binding
				.AddBinding(ViewModel, vm => vm.ShowSaveAddres, w => w.Visible).InitializeFromSource();
			ybuttonSend.Clicked += (o, s) => ViewModel.Send();
		}
	}
}
