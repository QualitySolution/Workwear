using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;

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
            EmployeeEntry = builder.ForProperty(x => x.Employee).MakeByType().Finish();
            SubdivisionEntry = builder.ForProperty(x => x.Subdivision).MakeByType().Finish();
            Date = DateTime.Today;
        }
        private EmployeeCard employee;
        public EmployeeCard Employee {
            get => employee;
            set {
	            if(SetField(ref employee, value))
		            if(employee != null) {
			            SubdivisionSensitive = false;
			            Subdivision = employee.Subdivision;
		            }
		            else 
			            SubdivisionSensitive = true;
            }
        }
        private Subdivision subdivision;
        public Subdivision Subdivision {
            get => subdivision;
            set => SetField(ref subdivision, value);
        }
        private DateTime date;
        public DateTime Date {
            get => date;
            set => SetField(ref date, value);
        }
        private bool dateSensitive;
        public bool DateSensitive {
            get => dateSensitive;
            set => SetField(ref dateSensitive, value);
        }
        private bool employeeSensitive;
        public bool EmployeeSensitive {
            get => employeeSensitive;
            set => SetField(ref employeeSensitive, value);
        }
        private bool subdivisionSensitive = true;
        public bool SubdivisionSensitive {
            get => subdivisionSensitive;
            set => SetField(ref subdivisionSensitive, value);
        }
        public EntityEntryViewModel<EmployeeCard> EmployeeEntry;
        public EntityEntryViewModel<Subdivision> SubdivisionEntry;
    }
}
