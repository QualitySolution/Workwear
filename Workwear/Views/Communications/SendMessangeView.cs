using Gamma.Utilities;
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
			
			listcomboboxTemplates.SetRenderTextFunc<MessageTemplate>(x => x.Name);
			listcomboboxTemplates.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Templates, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedTemplate, w => w.SelectedItem)
				.InitializeFromSource();
			entryTitle.Binding.AddBinding(ViewModel, vm => vm.MessageTitle, w => w.Text).InitializeFromSource();
			textTemplate.Binding.AddBinding(ViewModel, vm => vm.MessageText, w => w.Buffer.Text).InitializeFromSource();
			
			buttonSend.Binding.AddBinding(ViewModel, vm => vm.SensitiveSendButton, w => w.Sensitive).InitializeFromSource();
			pushCheckBox.Label += " " + ViewModel.AvailabelPushNotificationCount;
			pushCheckBox.Binding.AddBinding(ViewModel, vm => vm.PushNotificationSelected, w => w.Active).InitializeFromSource();
			emailCheckBox.Label += " " + ViewModel.AvailabelEmailNotificationCount;
			emailCheckBox.Binding.AddBinding(ViewModel, vm => vm.EmailNotificationSelected, w => w.Active).InitializeFromSource();
			
			ConfigureLinkGroup();
			ConfigureFileGroup();

			entryMessageTextToLongError.ForegroundColor = "#F00";
			entryMessageTextToLongError.Binding.AddBinding(ViewModel, vm => vm.MessageTextToLongError, w => w.Text).InitializeFromSource();
			
			entryMessageTextLengthHint.Binding.AddSource(ViewModel)
			   .AddBinding(vm => vm.MessageTextToLongHint, w => w.Text)
			   .AddBinding(vm => vm.ColorMessageTextToLongHint, w => w.ForegroundColor)
			   .InitializeFromSource();
		}

		private void ConfigureLinkGroup()
		{
			linkAttachCheckBox.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.VisibleLinkAttach, w => w.Visible)
				.AddBinding(vm => vm.LinkAttachSelected, w => w.Active).InitializeFromSource();
			
			labelLinkTitle.Binding.AddBinding(ViewModel, vm => vm.LinkAttachSelected, w => w.Visible).InitializeFromSource();
			entryLinkTitle.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.LinkTitleText, w => w.Text)
				.AddBinding(vm => vm.LinkAttachSelected, w => w.Visible).InitializeFromSource();
			
			labelLink.Binding.AddBinding(ViewModel, vm => vm.LinkAttachSelected, w => w.Visible).InitializeFromSource();
			entryLink.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.LinkText, w => w.Text)
				.AddBinding(vm => vm.LinkAttachSelected, w => w.Visible).InitializeFromSource();
		}

		private void ConfigureFileGroup() 
		{
			fileAttachCheckBox.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.VisibleFileAttach, w => w.Visible)
				.AddBinding(vm => vm.FileAttachSelected, w => w.Active).InitializeFromSource();
			
			labelFilename.Binding.AddBinding(ViewModel, vm => vm.FileAttachSelected, w => w.Visible).InitializeFromSource();
			entryFilename.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Filename, w => w.Text)
				.AddBinding(vm => vm.FileAttachSelected, w => w.Visible).InitializeFromSource();
			
			listComboBoxReports.SetRenderTextFunc<EmailDocument>(x => x.GetEnumTitle());
			listComboBoxReports.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FileAttachSelected, w => w.Visible)
				.AddBinding(vm => vm.Documents, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedReport, w => w.SelectedItem)
				.InitializeFromSource();
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
