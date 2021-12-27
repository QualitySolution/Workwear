using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using workwear.Domain.Tools;

namespace workwear.ViewModels.Tools
{
	public class SendMessangeViewModel: WindowDialogViewModelBase
	{
		IUnitOfWork Uow;
			
		public SendMessangeViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation): base(navigation)
		{
			Uow = unitOfWorkFactory.CreateWithoutRoot();
			Templates = Uow.GetAll<MessageTemplate>().ToList();
		}

		public void SendMessange()
		{
			Close(false, CloseSource.Self);
		}
		#region Sensetive
		public bool SensitiveSendButton {
			get => !String.IsNullOrWhiteSpace(MessageText) && !String.IsNullOrWhiteSpace(messageTitle);
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
	}
}
