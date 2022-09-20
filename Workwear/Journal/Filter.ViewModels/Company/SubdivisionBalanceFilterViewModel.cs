using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;

namespace workwear.Journal.Filter.ViewModels.Company
{
    public class SubdivisionBalanceFilterViewModel : JournalFilterViewModelBase<SubdivisionBalanceFilterViewModel>
    {
        public SubdivisionBalanceFilterViewModel(
            JournalViewModelBase journalViewModel,
            INavigationManager navigation, 
            ILifetimeScope autofacScope, 
            IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
        {
            var builder = new CommonEEVMBuilderFactory<SubdivisionBalanceFilterViewModel>(
                journalViewModel, this, UoW, navigation, autofacScope);
            SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
                .MakeByType()
                .Finish();
            Date = DateTime.Today;
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
        public EntityEntryViewModel<Subdivision> SubdivisionEntry;
    }
}
