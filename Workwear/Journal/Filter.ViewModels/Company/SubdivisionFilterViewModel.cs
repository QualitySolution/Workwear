using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace workwear.Journal.Filter.ViewModels.Company
{
    public class SubdivisionFilterViewModel : JournalFilterViewModelBase<EmployeeFilterViewModel>
    {
        public SubdivisionFilterViewModel(
            JournalViewModelBase journalViewModel, 
            IUnitOfWorkFactory unitOfWorkFactory = null
            ) : base(journalViewModel, unitOfWorkFactory)
        {
        }
        #region Ограничение
        private bool sortByParent;
        public bool SortByParent {
            get => sortByParent;
            set => SetField(ref sortByParent, value);
        }
        #endregion
    }
}