using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using QS.ViewModels.Extension;
using RdlEngine;
using Workwear.Domain.Communications;
using Workwear.Domain.Company;
using Workwear.Tools;

namespace Workwear.ViewModels.Communications 
{
	public class SendMessangeViewModel : WindowDialogViewModelBase, IDialogDocumentation
	{
		private readonly IList<EmployeeCard> employees;
		private readonly int? warehouseId;
		private readonly DateTime? endDateIssue;
		private readonly int[] protectionToolsIds;
		private readonly EmailManagerService emailManagerService;
		private readonly NotificationManagerService notificationManager;
		private readonly IInteractiveMessage interactive;
		private readonly ModalProgressCreator progressCreator;
		private readonly MySqlConnectionStringBuilder connectionStringBuilder;
		private readonly IGuiDispatcher guiDispatcher;

		readonly IUnitOfWork uow;

		public SendMessangeViewModel(int[] employeeIds, int warehouseId, DateTime? endDateIssue, int[] protectionToolsIds,
			IUnitOfWorkFactory unitOfWorkFactory, EmailManagerService emailManagerService,
			NotificationManagerService notificationManager, IInteractiveMessage interactive,
			ModalProgressCreator progressCreator, MySqlConnectionStringBuilder connectionStringBuilder,
			IGuiDispatcher guiDispatcher,
			INavigationManager navigation) : base(navigation)
		{
			this.warehouseId = warehouseId;
			this.endDateIssue = endDateIssue;
			this.protectionToolsIds = protectionToolsIds;
			this.emailManagerService = emailManagerService ?? throw new ArgumentNullException(nameof(emailManagerService));
			this.notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			this.connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
			this.guiDispatcher = guiDispatcher;
			Title = "Отправка уведомлений " + EmployeesToText(employeeIds.Length);

			uow = unitOfWorkFactory.CreateWithoutRoot();

			Templates = uow.GetAll<MessageTemplate>().ToList();
			employees = uow.GetById<EmployeeCard>(employeeIds);
			Documents = Enum.GetValues(typeof(EmailDocument))
				.Cast<EmailDocument>()
				.ToList();
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("mobile-app.html#send-notification");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip("Отправка уведомлений");
		#endregion

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

		public async Task SendMessageAsync()
		{
			List<string> responseMessages = new List<string>();
			IProgress<int> progress = new Progress<int>(val => 
			{
				if (progressCreator.IsStarted) 
				{
					progressCreator.Update(val);
					progressCreator.Update($"Отправлено: {val}");
				}
			});
			
			if (PushNotificationSelected) 
			{
				responseMessages.Add(SendPushNotificationMessage());
			}
			if (EmailNotificationSelected) 
			{
				responseMessages.Add(await SendEmailNotificationMessageAsync(progress));
			}
			
			guiDispatcher.RunInGuiTread(() => 
				interactive.ShowMessage(ImportanceLevel.Info, string.Join("\n", responseMessages))
			);
			
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

		private async Task<string> SendEmailNotificationMessageAsync(IProgress<int> progress = default)
		{
			IList<EmployeeCard> availableEmp = GetAvailableEmployeeWithEmail();
			string result = "Отправлено 0 email уведомлений.";

			int emailAmount = availableEmp.Count();
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			CancellationToken token = tokenSource.Token;
			progressCreator.Title = "Отправка email";
			progressCreator.UserCanCancel = true;
			progressCreator.Start(emailAmount);
			progressCreator.Canceled += (sender, args) => tokenSource.Cancel();
			
			if (emailAmount > 0) 
			{
				try 
				{
					IEnumerable<EmailMessage> messages = availableEmp.Select(MakeEmailMessage);
					result = await emailManagerService.SendMessagesAsync(messages, progress, token);
				}
				catch (OperationCanceledException) 
				{
					result = $"Операция отправки email уведолмений прервана.\nБыло отправлено {progressCreator.Value} email";
				}
			}

			progressCreator.Close();
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

			byte[] bytes = ConvertReportToByte(reportInfo, new Dictionary<string, object>() {
				{ "id", employee.Id },
				{ "warehouse_id", warehouseId },
				{ "endDateIssue", endDateIssue ?? DateTime.Now },
				{ "protection_tools_ids", protectionToolsIds }
			});
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
			IEnumerable<string> strings = parameters.Select(p => $"{p.Key}={ConvertParameterToString(p.Value)}");
			string stringParameters = string.Join("&", strings);
				using(MemoryStream ms =
			      ReportExporter.ExportToMemoryStream(reportInfo.GetReportUri(), stringParameters, 
				      connectionStringBuilder.ConnectionString, OutputPresentationType.PDF))
			{
				return ms.ToArray();
			}
		}

		private string ConvertParameterToString(object value) 
		{
			if (!(value is string) && value is IEnumerable values)
			{
				var valuesList = values.Cast<object>();
				return String.Join(",", valuesList);
			}

			if(value is DateTime dateTime) 
			{
				return dateTime.ToString("u");
			}
			
			return value.ToString();
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
