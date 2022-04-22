using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.Sizes;
using Workwear.Measurements;

namespace workwear.Journal.Filter.ViewModels.Sizes
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
			SensitiveSizeType = true;
			SizeService = sizeService;
		}
		#region Ограничения
		private SizeType selectedSizeType;
		public SizeType SelectedSizeType {
			get => selectedSizeType;
			set => SetField(ref selectedSizeType, value);
		}

		private bool sensitiveSizeTypeSizeType;
		public bool SensitiveSizeType {
			get => sensitiveSizeTypeSizeType;
			set => SetField(ref sensitiveSizeTypeSizeType, value);
		}
		#endregion
	}
}
