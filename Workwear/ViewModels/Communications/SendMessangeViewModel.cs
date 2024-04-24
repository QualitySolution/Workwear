using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using fyiReporting.RDL;
using Gamma.Utilities;
using Google.Protobuf;
using MySqlConnector;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Utilities;
using QS.ViewModels.Dialog;
using RdlEngine;
using Workwear.Domain.Communications;
using Workwear.Domain.Company;

namespace Workwear.ViewModels.Communications 
{
	public class SendMessangeViewModel : WindowDialogViewModelBase 
	{
		private readonly EmailManagerService emailManagerService;
		private readonly NotificationManagerService notificationManager;
		private readonly IInteractiveMessage interactive;
		private readonly MySqlConnectionStringBuilder connectionStringBuilder;

		readonly IUnitOfWork uow;
		readonly IList<EmployeeCard> employees;

		public SendMessangeViewModel(IUnitOfWorkFactory unitOfWorkFactory, int[] employeeIds, EmailManagerService emailManagerService,
			NotificationManagerService notificationManager, IInteractiveMessage interactive,
			MySqlConnectionStringBuilder connectionStringBuilder,
			INavigationManager navigation) : base(navigation)
		{
			this.emailManagerService = emailManagerService ?? throw new ArgumentNullException(nameof(emailManagerService));
			this.notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
			Title = "Отправка уведомлений " + EmployeesToText(employeeIds.Length);

			uow = unitOfWorkFactory.CreateWithoutRoot();

			Templates = uow.GetAll<MessageTemplate>().ToList();
			employees = uow.GetById<EmployeeCard>(employeeIds);
			Documents = Enum.GetValues(typeof(EmailDocument))
				.Cast<EmailDocument>()
				.ToList();
		}

		#region View Properties
		public bool SensitiveSendButton =>
			!string.IsNullOrWhiteSpace(MessageText) && !string.IsNullOrWhiteSpace(MessageTitle) &&
			(PushNotificationSelected || EmailNotificationSelected) && ValidMessageTextLength() &&
			((LinkAttachSelected == false && fileAttachSelected == false) || 
			 (LinkAttachSelected && !string.IsNullOrWhiteSpace(LinkTitleText) && !string.IsNullOrWhiteSpace(LinkText)) || 
			 (FileAttachSelected && !string.IsNullOrWhiteSpace(Filename)));
		
		public bool VisibleLinkAttach => PushNotificationSelected && !EmailNotificationSelected;

		public bool VisibleFileAttach => !PushNotificationSelected && EmailNotificationSelected;
		
		
		public string AvailabelPushNotificationCount => 
			$"(Возможно отправить {EmployeesToText(GetAvailableEmployeeWithPush().Count)})";

		public string AvailabelEmailNotificationCount =>
			$"(Возможно отправить {EmployeesToText(GetAvailableEmployeeWithEmail().Count)})";
		#endregion

		#region Parametrs

		public List<MessageTemplate> Templates { get; set; }

		private MessageTemplate selectedTemplate;
		public MessageTemplate SelectedTemplate 
		{
			get => selectedTemplate;
			set {
				MessageText = value.MessageText;
				MessageTitle = value.MessageTitle;
				LinkTitleText = value.LinkTitleText;
				LinkText = value.LinkText;
				selectedTemplate = value;
			}
		}

		public List<EmailDocument> Documents { get; set; }

		private EmailDocument selectedDocument;
		public EmailDocument SelectedReport
		{
			get => selectedDocument;
			set {
				Filename = value.GetAttribute<DisplayAttribute>()?.Name;
				SetField(ref selectedDocument, value);
			}
		}

		private string messageTitle;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public string MessageTitle 
		{
			get => messageTitle;
			set => SetField(ref messageTitle, value);
		}

		private bool pushNotificationSelected;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		[PropertyChangedAlso(nameof(VisibleLinkAttach))]
		[PropertyChangedAlso(nameof(VisibleFileAttach))]
		[PropertyChangedAlso(nameof(MessageTextToLongHint))]
		[PropertyChangedAlso(nameof(ColorMessageTextToLongHint))]
		[PropertyChangedAlso(nameof(MessageTextToLongError))]
		public bool PushNotificationSelected 
		{
			get => pushNotificationSelected;
			set {
				FileAttachSelected = false;
				SetField(ref pushNotificationSelected, value);
			}
		}

		private bool emailNotificationSelected;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		[PropertyChangedAlso(nameof(VisibleLinkAttach))]
		[PropertyChangedAlso(nameof(VisibleFileAttach))]
		[PropertyChangedAlso(nameof(MessageTextToLongHint))]
		[PropertyChangedAlso(nameof(ColorMessageTextToLongHint))]
		[PropertyChangedAlso(nameof(MessageTextToLongError))]
		public bool EmailNotificationSelected
		{
			get => emailNotificationSelected;
			set {
				LinkAttachSelected = false;
				SetField(ref emailNotificationSelected, value);
			}
		}

		private bool linkAttachSelected;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public bool LinkAttachSelected 
		{
			get => linkAttachSelected;
			set => SetField(ref linkAttachSelected, value);
		}

		private bool fileAttachSelected;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]

		public bool FileAttachSelected 
		{
			get => fileAttachSelected;
			set => SetField(ref fileAttachSelected, value);
		}

		private string messageText;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		[PropertyChangedAlso(nameof(MessageTextToLongHint))]
		[PropertyChangedAlso(nameof(ColorMessageTextToLongHint))]
		[PropertyChangedAlso(nameof(MessageTextToLongError))]
		public string MessageText 
		{
			get => messageText;
			set => SetField(ref messageText, value);
		}

		private string linkTitleText;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public string LinkTitleText 
		{
			get => linkTitleText;
			set => SetField(ref linkTitleText, value);
		}

		private string linkText;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public string LinkText 
		{
			get => linkText;
			set => SetField(ref linkText, value);
		}

		private string filename;
		[PropertyChangedAlso(nameof(SensitiveSendButton))]
		public string Filename 
		{
			get => filename;
			set => SetField(ref filename, value);
		}
		
		public string MessageTextToLongHint  => $"{MessageText?.Length ?? 0}/{GetMaxMessageTextLength()}";

		public string ColorMessageTextToLongHint => ValidMessageTextLength() ? 
			"#000" : "#F00";
		
		public string MessageTextToLongError 
		{
			get 
			{
				if (ValidMessageTextLength()) return string.Empty;

				int max = GetMaxMessageTextLength();
				if (PushNotificationSelected) 
				{
					return $"Длина текста в push уведомелнии не должна превышать {max} символов";
				}
				if (EmailNotificationSelected) 
				{
					return $"Длина текста в email уведомелнии не должна превышать {max} символов";
				}

				return null;
			}
		}
		#endregion

		#region Commands

		public void SendMessage()
		{
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

		private string SendPushNotificationMessage() 
		{
			IEnumerable<OutgoingMessage> messages = GetAvailableEmployeeWithPush().Select(MakeNotificationMessage);
			string result = $"Отправлено 0 push уведомлений.";
			if (messages.Any()) 
			{
				result = notificationManager.SendMessages(messages);
			}

			return result;
		}

		private string SendEmailNotificationMessage()
		{
			IEnumerable<EmailMessage> messages = GetAvailableEmployeeWithEmail().Select(MakeEmailMessage);
			string result = "Отправлено 0 email уведомлений.";
			if (messages.Any()) 
			{
				result = emailManagerService.SendMessages(messages);
			}

			return result;
		}

		#endregion

		#region Private Methods
		
		private OutgoingMessage MakeNotificationMessage(EmployeeCard employee)
		{
			OutgoingMessage message = new OutgoingMessage {
				Phone = employee.PhoneNumber,
				Title = MessageTitle,
				Text = MessageText,
				LinkTitle = LinkAttachSelected ? LinkTitleText : "",
				Link = LinkAttachSelected ? LinkText : "",
			};

			return message;
		}

		private EmailMessage MakeEmailMessage(EmployeeCard employee) 
		{
			EmailMessage message = new EmailMessage()
			{
				Address = employee.Email,
				Subject = MessageTitle,
				Text = MessageText
			};

			if(!FileAttachSelected) return message;
			
			ReportInfo reportInfo = new ReportInfo
			{
				Title = $"Карточка {employee.ShortName} - {selectedDocument.GetEnumTitle()}",
				Identifier = selectedDocument.GetAttribute<ReportIdentifierAttribute>().Identifier
			};

			byte[] bytes = ConvertReportToByte(reportInfo, new Dictionary<string, object>() { { "id", employee.Id } });
			message.Files.Add(new Attachment() 
				{
					FileName = $"{Filename}.pdf",
					File = ByteString.CopyFrom(bytes)
				}
			);
			
			return message;
		}

		private byte[] ConvertReportToByte(ReportInfo reportInfo, Dictionary<string, object> parameters) 
		{
			IEnumerable<string> strings = parameters.Select(p => $"{p.Key}={p.Value}");
			string stringParameters = string.Join("&", strings);
			using(MemoryStream ms =
			      ReportExporter.ExportToMemoryStream(reportInfo.GetReportUri(), stringParameters, 
				      connectionStringBuilder.ConnectionString, OutputPresentationType.PDF))
			{
				return ms.ToArray();
			}
		}

		private bool ValidMessageTextLength() 
		{
			int length = MessageText?.Length ?? 0;
			return length <= GetMaxMessageTextLength();
		}
		
		private int GetMaxMessageTextLength() 
		{
			int result = 400;
			if(!PushNotificationSelected && EmailNotificationSelected) 
			{
				result = 65535;
			}

			return result;
		}

		private string EmployeesToText(int count) 
		{
			return NumberToTextRus.FormatCase(count, "{0} сотруднику", "{0} сотрудникам", "{0} сотрудникам");
		}

		private IList<EmployeeCard> GetAvailableEmployeeWithPush() 
		{
			return employees.Where(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).ToList();
		}

		private IList<EmployeeCard> GetAvailableEmployeeWithEmail() 
		{
			return employees.Where(x => !string.IsNullOrWhiteSpace(x.Email)).ToList();
		}
		#endregion
	}
	
	public enum EmailDocument
	{
		[Display(Name = "СИЗ к получению")]
		[ReportIdentifier("Employee.IssuedSheet")]
		IssuedSheet
	}
}
