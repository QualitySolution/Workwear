using System;
using QS.Cloud.Email.DesktopClient;
using QS.Dialog;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;

namespace Workwear.ViewModels.Communications {
	
	public delegate void SetAddress (string address);
	public class SendEmailViewModel : WindowDialogViewModelBase {
		
		private readonly DesktopEmailSenderClient emailSenderClient;
		private readonly IInteractiveService interactive;
		
		public SendEmailViewModel(
			DesktopEmailSenderClient emailSenderClient,
			IInteractiveService interactive,
			INavigationManager navigation
			) : base(navigation)
		{
			this.emailSenderClient = emailSenderClient ?? throw new ArgumentNullException(nameof(emailSenderClient));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
        }
		
		private string title = "Отправка сообщения";
		public override string Title {
			get => title;
			set => SetField(ref title, value);
		}
		
		private string emailAddress;
		public virtual string EmailAddress {
			get => emailAddress;
			set => SetField(ref emailAddress, value);
		}
		
		private string topic;
		public virtual string Topic {
			get => topic;
			set => SetField(ref topic, value);
		}
		private string message;
		public virtual string Message {
			get => message;
			set => SetField(ref message, value);
		}
		private bool showSaveAddress;
		public virtual bool ShowSaveAddress {
			get => showSaveAddress;
			set => SetField(ref showSaveAddress, value);
		}
		
		public SetAddress SaveAddressFunc;

		public void SaveAddress() =>
			SaveAddressFunc(EmailAddress);

		public void Send() {
			string result = String.Empty;

			if(!EmailHelper.Validate(EmailAddress, false))
				result += "Некорректный формат email адреса. ";
			if(Topic.Length > 254) 
				result += "Тема не должна превышать 254 символа.";
			if(result != String.Empty) {
				interactive.ShowMessage(ImportanceLevel.Error,result);
				return;
			}
			
			try {
				result = emailSenderClient.SendMessage(EmailAddress, Topic, Message);
			}
			catch (OperationCanceledException) {
				result = $"Операция отправки email уведомлений прервана.";
			}

			interactive.ShowMessage(ImportanceLevel.Info,result,"Отправка");
			Close(false,CloseSource.Self);
		}
	}
}
