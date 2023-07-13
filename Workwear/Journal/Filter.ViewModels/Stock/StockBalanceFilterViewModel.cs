using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class StockBalanceFilterViewModel : JournalFilterViewModelBase<StockBalanceFilterViewModel>
	{
		private readonly CurrentUserSettings currentUserSettings;
		
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

		private DateTime date;
		public virtual DateTime Date {
			get => date;
			set => SetField(ref date, value);
		}

		private AddedAmount addAmount; 
		public virtual AddedAmount AddAmount {
			get => addAmount;
			set {
				if(addAmount != value) {
					currentUserSettings.Settings.DefaultAddedAmount = value;
					currentUserSettings.SaveSettings();
					SetField(ref addAmount, value);
				}
			}
		}
		#endregion

		public readonly FeaturesService FeaturesService;

		#region Visible

		public bool VisibleWarehouse => FeaturesService.Available(WorkwearFeature.Warehouses);
		private bool canChooseAmount = false;
		public bool CanChooseAmount {
			get => canChooseAmount;
			set => SetField(ref canChooseAmount, value);
		}
		#endregion

		public EntityEntryViewModel<Warehouse> WarehouseEntry;

		public StockBalanceFilterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			JournalViewModelBase journal,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			StockRepository stockRepository,
			FeaturesService featuresService,CurrentUserSettings currentUserSettings): base(journal, unitOfWorkFactory)
		{
			FeaturesService = featuresService;
			this.currentUserSettings = currentUserSettings;
			date = DateTime.Today;

			var builder = new CommonEEVMBuilderFactory<StockBalanceFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			addAmount = currentUserSettings.Settings.DefaultAddedAmount;
			
			WarehouseEntry = builder.ForProperty(x => x.Warehouse).UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}
	}
}
