using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.Sizes;
using Workwear.Measurements;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class SizeFilterViewModel: JournalFilterViewModelBase<SizeFilterViewModel>
	{
		public SizeService SizeService { get; }
		public SizeFilterViewModel(
			JournalViewModelBase journalViewModel,
			SizeService sizeService,
			IUnitOfWorkFactory unitOfWorkFactory = null
			) : base(journalViewModel, unitOfWorkFactory)
		{
			Sensitive = true;
			SizeService = sizeService;
		}
		#region Ограничения
		private SizeType selectedSizeType;
		public SizeType SelectedSizeType {
			get => selectedSizeType;
			set => SetField(ref selectedSizeType, value);
		}

		private bool sensitive;
		public bool Sensitive {
			get => sensitive;
			set => SetField(ref sensitive, value);
		}
		#endregion
	}
}
