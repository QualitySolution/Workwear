using System;
using System.Collections.Generic;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;

namespace Workwear.ViewModels.Communications
{
	public class HistoryNotificationViewModel: UowDialogViewModelBase, IDialogDocumentation
	{
		public IList<MessageItem> MessageItems { get; }
		private readonly string employeePhone;
		public HistoryNotificationViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			MessagesService messagesService,
			int employeeId) : base(unitOfWorkFactory, navigation)
		{
			var employee = UoW.GetById<EmployeeCard>(employeeId);
			employeePhone = employee.PhoneNumber;
			MessageItems = !String.IsNullOrEmpty(employeePhone) 
				? messagesService.GetMessages(employeePhone) 
				: new List<MessageItem>();
			Title = $"Сообщения: {employee.Title}";
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("mobile-app.adoc#notification-log");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip(Title);
		#endregion
	}
}
