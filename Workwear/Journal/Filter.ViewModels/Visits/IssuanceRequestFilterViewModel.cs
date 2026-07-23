using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.Visits;

namespace Workwear.Journal.Filter.ViewModels.Visits {
	public class IssuanceRequestFilterViewModel: JournalFilterViewModelBase<IssuanceRequestFilterViewModel> {
		public IssuanceRequestFilterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			JournalViewModelBase journal
			): base(journal, unitOfWorkFactory) 
		{
			
		}

		#region Ограничения

		private IssuanceRequestStatus? status;
		public virtual IssuanceRequestStatus? Status {
			get => status;
			set => SetField(ref status, value);
		}

		#endregion
	}
}
