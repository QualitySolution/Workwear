using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Tools.Features;
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

		private ProtectionTools protectionTools;
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		public readonly FeaturesService FeaturesService;

		#endregion

		public EntityEntryViewModel<Warehouse> WarehouseEntry;

		public StockBalanceFilterViewModel(IUnitOfWorkFactory unitOfWorkFactory, JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, FeaturesService featuresService): base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<StockBalanceFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			FeaturesService = featuresService;

			WarehouseEntry = builder.ForProperty(x => x.Warehouse).UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}


	}
}
