using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;

namespace Workwear.ViewModels.Company {
	public class CostCenterViewModel : EntityDialogViewModelBase<CostCenter>
	{
		public CostCenterViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IValidator validator
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{

		}
	}
}
