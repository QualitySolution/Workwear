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

namespace workwear.ViewModels.Communications
{
	public class SendMessangeViewModel: WindowDialogViewModelBase
	{
		private readonly NotificationManagerService notificationManager;
		private readonly IInteractiveMessage interactive;

		readonly IUnitOfWork uow;
		readonly IList<EmployeeCard> employees;
			
		public SendMessangeViewModel(IUnitOfWorkFactory unitOfWorkFactory, int[] employeeIds, NotificationManagerService notificationManager, IInteractiveMessage interactive, INavigationManager navigation): base(navigation)
		{
			this.notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			Title = "Отправка уведомлений " + NumberToTextRus.FormatCase(employeeIds.Length, "{0} сотруднику", "{0} сотрудникам", "{0} сотрудникам");

			uow = unitOfWorkFactory.CreateWithoutRoot();

			Templates = uow.GetAll<MessageTemplate>().ToList();
			employees = uow.GetById<EmployeeCard>(employeeIds);
		}

		#region Sensetive
		public bool SensitiveSendButton {
			get => !String.IsNullOrWhiteSpace(MessageText) && !String.IsNullOrWhiteSpace(MessageTitle);
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
		public void SendMessage()
		{
			var messages = new List<OutgoingMessage>();
			foreach (var employee in employees) {
				messages.Add(MakeMessage(employee));
			}
			
			var result = notificationManager.SendMessages(messages);
			interactive.ShowMessage(ImportanceLevel.Info, result);
			
			Close(false, CloseSource.Self);
		}
		#endregion

		private OutgoingMessage MakeMessage(EmployeeCard employee)
		{
			var message = new OutgoingMessage {
				Phone = employee.PhoneNumber,
				Title = MessageTitle,
				Text = MessageText
			};

			return message;
		}
	}
}
