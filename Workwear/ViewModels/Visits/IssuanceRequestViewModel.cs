using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Visits;

namespace Workwear.ViewModels.Visits {
	public class IssuanceRequestViewModel: EntityDialogViewModelBase<IssuanceRequest> {
		public IssuanceRequestViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IValidator validator = null): base(uowBuilder, unitOfWorkFactory, navigation, validator) {
			
		}
	}
}
