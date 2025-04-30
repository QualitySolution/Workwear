using System;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;

namespace Workwear.ViewModels.Communications {
	
	public delegate void SetAdress (string adress);
	public class SendEmailViewModel : WindowDialogViewModelBase {
		
		private readonly EmailManagerService emailManagerService;
		private readonly IInteractiveService interactive;
		
		public SendEmailViewModel(
			EmailManagerService emailManagerService,
			IInteractiveService interactive,
			INavigationManager navigation
			) : base(navigation)
		{
			this.emailManagerService = emailManagerService ?? throw new ArgumentNullException(nameof(emailManagerService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
        }
		
		private string title = "Отправка сообщения";
		public override string Title {
			get => title;
			set => SetField(ref title, value);
		}
		
		private string emailAddres;
		public virtual string EmailAddres {
			get => emailAddres;
			set => SetField(ref emailAddres, value);
		}
		
		private string topic;
		public virtual string Topic {
			get => topic;
			set => SetField(ref topic, value);
		}
		private string messege;
		public virtual string Messege {
			get => messege;
			set => SetField(ref messege, value);
		}
		private bool showSaveAddres;
		public virtual bool ShowSaveAddres {
			get => showSaveAddres;
			set => SetField(ref showSaveAddres, value);
		}
		
		public SetAdress SaveAdressFunc;

		public void SaveAdress() =>
			SaveAdressFunc(EmailAddres);

		public void Send() {
			string result = String.Empty;

			if(!EmailHelper.Validate(EmailAddres, false))
				result += "Некорректный формат email адреса. ";
			if(Topic.Length > 254) 
				result += "Тема не должна привышать 254 символа.";
			if(result != String.Empty) {
				interactive.ShowMessage(ImportanceLevel.Error,result);
				return;
			}
			
			try {
				result = emailManagerService.SendMessages(
					new[] {
						new EmailMessage() {
							Address = EmailAddres,
							Subject = Topic,
							Text = Messege
						}
					});
			}
			catch (OperationCanceledException) {
				result = $"Операция отправки email уведолмений прервана.";
			}

			interactive.ShowMessage(ImportanceLevel.Info,result,"Отпрвка");
			Close(false,CloseSource.Self);
		}
	}
}
