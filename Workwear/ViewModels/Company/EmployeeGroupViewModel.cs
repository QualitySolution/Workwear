using System;
using System.Linq;
using Autofac;
using MySqlConnector;
using NHibernate.Exceptions;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;

namespace Workwear.ViewModels.Company {
	public class EmployeeGroupViewModel : EntityDialogViewModelBase<EmployeeGroup> {
		private readonly IInteractiveMessage interactive;

		public EmployeeGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope autofacScope,
			INavigationManager navigation,
			IInteractiveMessage interactive,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			if(!UoW.IsNew) {
				var employeeIds = Entity.Items.Select(x => x.Employee.Id);
				UoW.GetById<EmployeeCard>(employeeIds);
			}
			
			var thisViewModel = new TypedParameter(typeof(EmployeeGroupViewModel), this);
			ItemsViewModel = autofacScope.Resolve<EmployeeGroupItemsViewModel>(thisViewModel);
		}

		#region Свойства
		public EmployeeGroupItemsViewModel ItemsViewModel { get; }

		private int currentTab = 1;
		public virtual int CurrentTab {
			get => currentTab;
			set => SetField(ref currentTab, value);
		}
		#endregion

		public override bool Save() {
			try {
				return base.Save();
			}
			catch(GenericADOException e) when(e.InnerException is MySqlException myExp && myExp.ErrorCode == MySqlErrorCode.DuplicateKeyEntry) {
				interactive.ShowMessage(ImportanceLevel.Error, 
					"Один или несколько сотрудников добавленных в группу уже присутствуют в группе. Возможно их добавил другой пользователь. Переоткройте диалог группы, чтобы увидеть изменения.");
				return false;
			}
		}
	}
}
