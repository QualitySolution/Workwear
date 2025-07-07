using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;

namespace Workwear.ViewModels.Company
{
    public class VacationTypeViewModel: EntityDialogViewModelBase<VacationType>, IDialogDocumentation
    {
        public VacationTypeViewModel(
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            INavigationManager navigation, 
            IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
        {
        }
        
        #region IDialogDocumentation
        public string DocumentationUrl => DocHelper.GetDocUrl("employees.html#vacations-types");
        public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
        #endregion
    }
}
