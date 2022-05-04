using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using Workwear.Tools;

namespace workwear.ViewModels.Company
{
    public class EmployeeVacationViewModel : EntityDialogViewModelBase<EmployeeVacation>
    {
        public EmployeeVacationViewModel(
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            INavigationManager navigation,
            EmployeeCard employee = null,
            IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
        {
            Entity.Employee = employee;
        }

        public override bool Save()
        {
            var baseParameters = new BaseParameters(UoW.Session.Connection);
            Entity.UpdateRelatedOperations(UoW, new Repository.Operations.EmployeeIssueRepository(), baseParameters, new GtkQuestionDialogsInteractive());
            UoW.Save(Entity.Employee);
            return base.Save();
        }
    }
}