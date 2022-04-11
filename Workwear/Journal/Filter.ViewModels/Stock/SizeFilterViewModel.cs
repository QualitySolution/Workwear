﻿using QS.DomainModel.UoW;
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
			Sensitive = true;
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
