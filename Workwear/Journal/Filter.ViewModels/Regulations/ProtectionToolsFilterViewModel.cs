using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace Workwear.Journal.Filter.ViewModels.Regulations {
	public partial class ProtectionToolsFilterViewModel : JournalFilterViewModelBase<ProtectionToolsFilterViewModel>
	{
		public ProtectionToolsFilterViewModel(
			JournalViewModelBase journal,
			IUnitOfWorkFactory unitOfWorkFactory = null
			) : base(journal, unitOfWorkFactory)
		{
		}
		private bool onlyDermal;
		public bool OnlyDermal {
			get => onlyDermal;
			set => SetField(ref onlyDermal, value);
		}
		private bool notDispenser;
		public bool NotDispenser {
			get => notDispenser;
			set => SetField(ref notDispenser, value);
		}
	}
}
