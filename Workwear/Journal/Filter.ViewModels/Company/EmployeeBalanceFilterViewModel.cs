using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Users;
using Workwear.Tools.User;

namespace workwear.Journal.Filter.ViewModels.Company
{
    public class EmployeeBalanceFilterViewModel : JournalFilterViewModelBase<EmployeeBalanceFilterViewModel>
    {
	    private readonly CurrentUserSettings currentUserSettings;
	    
        public EmployeeBalanceFilterViewModel(
            JournalViewModelBase journalViewModel,
            INavigationManager navigation, 
            ILifetimeScope autofacScope,
            CurrentUserSettings currentUserSettings,
            EmployeeCard employee = null,
            IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
        {
	        this.currentUserSettings = currentUserSettings;
	        CanNotify = false;
	        var builder = new CommonEEVMBuilderFactory<EmployeeBalanceFilterViewModel>(
                journalViewModel, this, UoW, navigation, autofacScope);
            EmployeeEntry = builder.ForProperty(x => x.Employee).MakeByType().Finish();
            SubdivisionEntry = builder.ForProperty(x => x.Subdivision).MakeByType().Finish();
            Date = DateTime.Today;
            addAmount = currentUserSettings.Settings.DefaultAddedAmount;
            Employee = employee;
            CanNotify = true;
        }
        private EmployeeCard employee;
        public EmployeeCard Employee {
            get => employee;
            set { if(SetField(ref employee, value))
		            if(value != null) {
			            SubdivisionSensitive = false;
			            Subdivision = employee.Subdivision;
		            }
		            else {
			            SubdivisionSensitive = true;
			            Subdivision = null;
		            }
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
        private bool checkShowAll;
        public bool CheckShowAll {
	        get => checkShowAll;
	        set => SetField(ref checkShowAll, value);
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
        private bool checkShowWriteoffVisible = true;
        public bool CheckShowWriteoffVisible {
	        get => checkShowWriteoffVisible;
	        set => SetField(ref checkShowWriteoffVisible, value);
        }
        private bool canChooseAmount = false;
        public bool CanChooseAmount {
	        get => canChooseAmount;
	        set => SetField(ref canChooseAmount, value);
        }
        private AddedAmount addAmount; 
        public virtual AddedAmount AddAmount {
	        get => addAmount;
	        set {
		        if(addAmount != value) {
			        currentUserSettings.Settings.DefaultAddedAmount = value;
			        currentUserSettings.SaveSettings();
			        SetField(ref addAmount, value);
		        }
	        }
        }
        
        public EntityEntryViewModel<EmployeeCard> EmployeeEntry;
        public EntityEntryViewModel<Subdivision> SubdivisionEntry;
    }
}
