using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.ClothingService;

namespace workwear.ViewModels.ClothingService {
	public class ServiceClaimViewModel : EntityDialogViewModelBase<ServiceClaim> {
		public ServiceClaimViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
		}
	}
}
