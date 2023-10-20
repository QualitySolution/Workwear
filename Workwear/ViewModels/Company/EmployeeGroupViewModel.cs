using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;

namespace Workwear.ViewModels.Company {
	public class EmployeeGroupViewModel : EntityDialogViewModelBase<EmployeeGroup> {
		public EmployeeGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope autofacScope,
			INavigationManager navigation,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
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
	}
}
