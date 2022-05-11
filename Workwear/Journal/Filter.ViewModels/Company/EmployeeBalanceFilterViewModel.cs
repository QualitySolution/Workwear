using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;

namespace workwear.Journal.Filter.ViewModels.Company
{
    public class EmployeeBalanceFilterViewModel : JournalFilterViewModelBase<EmployeeBalanceFilterViewModel>
    {
        public EmployeeBalanceFilterViewModel(
            JournalViewModelBase journalViewModel,
            INavigationManager navigation, 
            ILifetimeScope autofacScope, 
            IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
        {
            var builder = new CommonEEVMBuilderFactory<EmployeeBalanceFilterViewModel>(
                journalViewModel, this, UoW, navigation, autofacScope);
            EmployeeEntry = builder.ForProperty(x => x.Employee)
                .MakeByType()
                .Finish();
            Date = DateTime.Today;
        }
        private EmployeeCard employee;
        public EmployeeCard Employee {
            get => employee;
            set => SetField(ref employee, value);
        }
        private DateTime date;
        public DateTime Date {
            get => date;
            set => SetField(ref date, value);
        }
        public EntityEntryViewModel<EmployeeCard> EmployeeEntry;
    }
}