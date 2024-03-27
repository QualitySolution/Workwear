using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities;
using QS.ViewModels.Dialog;
using Workwear.Domain.Communications;
using Workwear.Domain.Company;

namespace Workwear.ViewModels.Communications
{
	public class SendMessangeViewModel: WindowDialogViewModelBase
	{
		private readonly EmailManagerService emailManagerService;
		private readonly NotificationManagerService notificationManager;
		private readonly IInteractiveMessage interactive;

		readonly IUnitOfWork uow;
		readonly IList<EmployeeCard> employees;
			
		public SendMessangeViewModel(IUnitOfWorkFactory unitOfWorkFactory, int[] employeeIds, EmailManagerService emailManagerService, NotificationManagerService notificationManager, IInteractiveMessage interactive, INavigationManager navigation): base(navigation)
		{
			this.emailManagerService = emailManagerService ?? throw new ArgumentNullException(nameof(emailManagerService));
			this.notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			Title = "Отправка уведомлений " + NumberToTextRus.FormatCase(employeeIds.Length, "{0} сотруднику", "{0} сотрудникам", "{0} сотрудникам");

			uow = unitOfWorkFactory.CreateWithoutRoot();

			Templates = uow.GetAll<MessageTemplate>().ToList();
			employees = uow.GetById<EmployeeCard>(employeeIds);
		}

		#region Sensetive
		public bool SensitiveSendButton {
			get => !String.IsNullOrWhiteSpace(MessageText) && !String.IsNullOrWhiteSpace(MessageTitle) &&
			       (PushNotificationSelected || EmailNotificationSelected);
		}
		#endregion

		#region Parametrs
		public List<MessageTemplate> Templates { get; set; }

		private MessageTemplate selectedTemplate;

		public MessageTemplate SelectedTemplate {
			get => selectedTemplate;
			set {
				MessageText = value.MessageText;
				MessageTitle = value.MessageTitle;
				selectedTemplate = value;
			}
		}

		private string messageTitle;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public string MessageTitle {
			get => messageTitle;
			set {
				SetField(ref messageTitle, value);
			}
		}

		private bool pushNotificationSelected;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public bool PushNotificationSelected {
			get => pushNotificationSelected;
			set => SetField(ref pushNotificationSelected, value);
		}

		private bool emailNotificationSelected;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public bool EmailNotificationSelected {
			get => emailNotificationSelected;
			set => SetField(ref emailNotificationSelected, value);
		}
		
		private string messageText;

		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public string MessageText {
			get => messageText;
			set {
				SetField(ref messageText, value);
			}
		}
		#endregion

		#region Действия View
		public void SendMessage() {
			List<string> responseMessages = new List<string>();
			if (PushNotificationSelected)
			{
				responseMessages.Add(SendPushNotificationMessage());
			}
			if (EmailNotificationSelected) 
			{
				responseMessages.Add(SendEmailNotificationMessage());
			}
			
			interactive.ShowMessage(ImportanceLevel.Info, string.Join("\n", responseMessages));
			Close(false, CloseSource.Self);
		}

		private string SendPushNotificationMessage() {
			IEnumerable<OutgoingMessage> messages = employees.Where(e => !string.IsNullOrWhiteSpace(e.PhoneNumber))
				.Select(MakeNotificationMessage);

			string result = $"Отправлено 0 сообщений.";  
			if (messages.Any()) 
			{
				result = notificationManager.SendMessages(messages);
			}

			return result;
		}

		private string SendEmailNotificationMessage()
		{
			IEnumerable<EmailMessage> messages = employees.Where(e => !string.IsNullOrWhiteSpace(e.Email))
				.Select(MakeEmailMessage);
			
			string result = "Отправлено 0 сообщений.";  
			if (messages.Any()) 
			{
				result = emailManagerService.SendMessages(messages);
			}

			return result;
		}
		#endregion

		private OutgoingMessage MakeNotificationMessage(EmployeeCard employee)
		{
			OutgoingMessage message = new OutgoingMessage {
				Phone = employee.PhoneNumber,
				Title = MessageTitle,
				Text = MessageText
			};

			return message;
		}
		
		private EmailMessage MakeEmailMessage(EmployeeCard employee)
		{
			EmailMessage message = new EmailMessage {
				Address = employee.Email,
				Subject = MessageTitle,
				Text = MessageText
			};

			return message;
		}
	}
}
