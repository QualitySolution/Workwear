using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace workwear.Journal.Filter.ViewModels.Statements
{
	public class IssuanceSheetFilterViewModel : JournalFilterViewModelBase<IssuanceSheetFilterViewModel>
	{
		#region Ограничения
		private DateTime? startDate;
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate;
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}
		#endregion

		public IssuanceSheetFilterViewModel(IUnitOfWorkFactory unitOfWorkFactory, JournalViewModelBase journal): base(journal, unitOfWorkFactory)
		{
		}
	}
}
