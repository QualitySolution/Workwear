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
    public class SubdivisionBalanceFilterViewModel : JournalFilterViewModelBase<SubdivisionBalanceFilterViewModel>
    {
	    private readonly CurrentUserSettings currentUserSettings;
	    
        public SubdivisionBalanceFilterViewModel(
            JournalViewModelBase journalViewModel,
            INavigationManager navigation, 
            ILifetimeScope autofacScope, CurrentUserSettings currentUserSettings, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
        {
	        this.currentUserSettings = currentUserSettings;
	        var builder = new CommonEEVMBuilderFactory<SubdivisionBalanceFilterViewModel>(
                journalViewModel, this, UoW, navigation, autofacScope);
            SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
                .MakeByType()
                .Finish();
            Date = DateTime.Today;
            addAmount = currentUserSettings.Settings.DefaultAddedAmount;
        }
        private DateTime date;
        public DateTime Date {
            get => date;
            set => SetField(ref date, value);
        }
        private Subdivision subdivision;
        public Subdivision Subdivision {
            get => subdivision;
            set => SetField(ref subdivision, value);
        }
        private bool dateSensitive;
        public bool DateSensitive {
            get => dateSensitive;
            set => SetField(ref dateSensitive, value);
        }
        private bool subdivisionSensitive;
        public bool SubdivisionSensitive {
            get => subdivisionSensitive;
            set => SetField(ref subdivisionSensitive, value);
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
        public EntityEntryViewModel<Subdivision> SubdivisionEntry;
    }
}
