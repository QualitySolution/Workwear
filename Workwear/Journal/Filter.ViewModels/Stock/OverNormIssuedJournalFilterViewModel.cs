using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.Operations;

namespace Workwear.Journal.Filter.ViewModels.Stock {
	public class OverNormIssuedJournalFilterViewModel : JournalFilterViewModelBase<OverNormIssuedJournalFilterViewModel> {
		public OverNormIssuedJournalFilterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			JournalViewModelBase journal,
			Action<OverNormIssuedJournalFilterViewModel> setFilterParameters = null
		) : base(journal, unitOfWorkFactory) {
			CanNotify = false;
			setFilterParameters?.Invoke(this);
			CanNotify = true;
		}

		private OverNormType? type;
		public virtual OverNormType? Type {
			get => type;
			set => SetField(ref type, value);
		}
	}
}
