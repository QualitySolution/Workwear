using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company {
	public class EmployeeGroupViewModel : EntityDialogViewModelBase<EmployeeGroup>, ISelectItem {
		public EmployeeGroupViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
		}

		public void SelectItem(int id) {
			throw new System.NotImplementedException();
		}
	}
}
