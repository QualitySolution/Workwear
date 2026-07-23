using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace Workwear.Journal.Filter.ViewModels.Regulations {
	public class DutyNormFilterViewModel : JournalFilterViewModelBase<DutyNormFilterViewModel> {
		public DutyNormFilterViewModel(
			JournalViewModelBase journalViewModelBase,
			IUnitOfWorkFactory unitOfWorkFactory
			) : base(journalViewModelBase, unitOfWorkFactory) {
			
		}

		#region Ограничения
		private bool showArchival;
		public bool ShowArchival {
			get => showArchival;
			set => SetField(ref showArchival, value);
		}
		#endregion
		
	}
}
