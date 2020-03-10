using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.JournalViewModels.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class StockBalanceFilterViewModel : JournalFilterViewModelBase<StockBalanceFilterViewModel>
	{
		#region Ограничения
		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}


		private bool showNegativeBalance;
		public virtual bool ShowNegativeBalance {
			get => showNegativeBalance;
			set => SetField(ref showNegativeBalance, value);
		}

		private ItemTypeCategory? itemTypeCategory;

		public virtual ItemTypeCategory? ItemTypeCategory {
			get => itemTypeCategory;
			set => SetField(ref itemTypeCategory, value);
		}

		#endregion

		public EntityEntryViewModel<Warehouse> WarehouseEntry;

		public StockBalanceFilterViewModel(IUnitOfWorkFactory unitOfWorkFactory, JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<StockBalanceFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			WarehouseEntry = builder.ForProperty(x => x.Warehouse).UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}

	}
}
