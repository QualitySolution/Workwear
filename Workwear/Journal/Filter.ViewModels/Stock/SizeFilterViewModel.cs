using QS.DomainModel.UoW;
using QS.Project.Journal;
using workwear.Domain.Sizes;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class SizeFilterViewModel: JournalFilterViewModelBase<SizeFilterViewModel>
	{
		public SizeFilterViewModel(
			JournalViewModelBase journalViewModel,
			IUnitOfWorkFactory unitOfWorkFactory = null
			) : base(journalViewModel, unitOfWorkFactory)
		{
		}
		#region Ограничения
		private SizeType sizeType;
		public SizeType SizeType {
			get => sizeType;
			set => SetField(ref sizeType, value);
		}
		#endregion
	}
}
