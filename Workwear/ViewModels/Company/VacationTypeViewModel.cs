using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;

namespace Workwear.ViewModels.Company
{
    public class VacationTypeViewModel: EntityDialogViewModelBase<VacationType>
    {
        public VacationTypeViewModel(
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            INavigationManager navigation, 
            IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
        {
        }
    }
}
