using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace workwear.Journal.Filter.ViewModels.Company
{
	public class EmployeeFilterViewModel : JournalFilterViewModelBase<EmployeeFilterViewModel>
	{
		#region Ограничения

		private bool showOnlyWork;
		public virtual bool ShowOnlyWork {
			get => showOnlyWork;
			set => SetField(ref showOnlyWork, value);
		}

		#endregion

		public EmployeeFilterViewModel(JournalViewModelBase journalViewModel, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
		{
		}
	}
}
