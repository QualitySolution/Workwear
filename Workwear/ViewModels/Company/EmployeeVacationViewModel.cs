using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.ViewModels.Company
{
    public class EmployeeVacationViewModel : EntityDialogViewModelBase<EmployeeVacation>
    {
        private readonly BaseParameters baseParameters;
        public EmployeeVacationViewModel(
            int employeeId, 
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            INavigationManager navigation, 
            BaseParameters baseParameters, 
            IValidator validator = null) : this(uowBuilder, unitOfWorkFactory, navigation, baseParameters, validator)
        {
            Entity.Employee = UoW.GetById<EmployeeCard>(employeeId);
        }
        public EmployeeVacationViewModel(
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            INavigationManager navigation,
            BaseParameters baseParameters,
            IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
        {
            this.baseParameters = baseParameters;
        }

        public override bool Save() {
            if (UoW.IsNew) {
                Entity.Employee.AddVacation(Entity);
            }
            Entity.UpdateRelatedOperations(UoW, new EmployeeIssueRepository(), baseParameters, new GtkQuestionDialogsInteractive());
            UoW.Save(Entity.Employee);
            return base.Save();
        }
    }
}
