using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace Workwear.Journal.Filter.ViewModels.ClothingService {
	public class ClaimsJournalFilterViewModel : JournalFilterViewModelBase<ClaimsJournalFilterViewModel> {
		
		public ClaimsJournalFilterViewModel(JournalViewModelBase journalViewModel, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
		{
		}
		
		#region Ограничения
		private bool showClosed;
		public virtual bool ShowClosed {
			get => showClosed;
			set => SetField(ref showClosed, value);
		}
		#endregion

		#region Sensetive
		private bool sensitiveShowClosed = true;
		public virtual bool SensitiveShowClosed {
			get => sensitiveShowClosed;
			set => SetField(ref sensitiveShowClosed, value);
		}
		#endregion
	}
}
