﻿using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Communications;

namespace Workwear.ViewModels.Communications
{
	public class MessageTemplateViewModel : EntityDialogViewModelBase<MessageTemplate>
	{
		public MessageTemplateViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
	}
}
