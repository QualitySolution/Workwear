﻿using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.Sizes;

namespace workwear.Journal.Filter.ViewModels.Sizes
{
	public class SizeTypeFilterViewModel: JournalFilterViewModelBase<SizeTypeFilterViewModel>
	{
		public SizeTypeFilterViewModel(
			JournalViewModelBase journalViewModel,
			IUnitOfWorkFactory unitOfWorkFactory = null
		) : base(journalViewModel, unitOfWorkFactory)
		{
		}
			#region Ограничения
			private CategorySizeType? category;
			public CategorySizeType? Category{
				get => category;
				set => SetField(ref category, value);
			}
			#endregion
	}
}
