using System;
using System.Linq;
using Autofac;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Domain.Users;
using workwear.Journal.ViewModels.Stock;

namespace workwear.ViewModels.User
{
	public class UserSettingsViewModel : EntityDialogViewModelBase<UserSettings>
	{
		public UserSettingsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator = null) 
		: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{

		}
	}
}
