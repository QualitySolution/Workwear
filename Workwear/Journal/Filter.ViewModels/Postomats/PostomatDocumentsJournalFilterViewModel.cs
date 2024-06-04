using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace Workwear.Journal.Filter.ViewModels.Postomats {
	public class PostomatDocumentsJournalFilterViewModel : JournalFilterViewModelBase<PostomatDocumentsJournalFilterViewModel> {
		
		public PostomatDocumentsJournalFilterViewModel(JournalViewModelBase journalViewModel, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
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
