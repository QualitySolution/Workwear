using System;
using System.Collections.Generic;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;

namespace workwear.ViewModels.Communications
{
	public class HistoryNotificationViewModel: UowDialogViewModelBase
	{
		public IList<MessageItem> MessageItems { get; }
		
		public HistoryNotificationViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			MessagesService messagesService,
			int employeeId,
			IValidator validator = null) : base(unitOfWorkFactory, navigation, validator)
		{
			var employee = UoW.GetById<EmployeeCard>(employeeId);
			MessageItems = !String.IsNullOrEmpty(employee.PhoneNumber) 
				? messagesService.GetMessages(employee.PhoneNumber) 
				: new List<MessageItem>();
			Title = $"Сообщения: {employee.Title}";
		}
	}
}
