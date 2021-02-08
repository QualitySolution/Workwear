using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Stock;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class NomenclatureFilterViewModel : JournalFilterViewModelBase<NomenclatureFilterViewModel>
	{
		public EntityEntryViewModel<ItemsType> EntryItemsType;

		public NomenclatureFilterViewModel(JournalViewModelBase journalViewModel, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journalViewModel, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<NomenclatureFilterViewModel>(journalViewModel, this, UoW, navigation, autofacScope);

			EntryItemsType = builder.ForProperty(x => x.ItemType).MakeByType().Finish();
		}

		#region Ограничения
		private ItemsType itemType;
		public virtual ItemsType ItemType {
			get => itemType;
			set => SetField(ref itemType, value);
		}
		#endregion
	}
}
