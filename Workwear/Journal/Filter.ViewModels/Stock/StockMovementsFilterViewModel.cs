using System;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class StockMovementsFilterViewModel : JournalFilterViewModelBase<StockMovementsFilterViewModel>
	{
		#region Ограничения
		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}

		private DateTime? startDate;
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate;
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		private StockPosition stockPosition;
		[PropertyChangedAlso(nameof(StockPositionTitle))]
		public virtual StockPosition StockPosition {
			get => stockPosition;
			set => SetField(ref stockPosition, value);
		}
		#endregion

		public readonly FeaturesService FeaturesService;

		public string StockPositionTitle => StockPosition?.Title;

		#region Visible

		public bool VisibleWarehouse => FeaturesService.Available(Tools.Features.WorkwearFeature.Warehouses);

		#endregion

		public EntityEntryViewModel<Warehouse> WarehouseEntry;

		public StockMovementsFilterViewModel(IUnitOfWorkFactory unitOfWorkFactory, JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, FeaturesService featuresService): base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<StockMovementsFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			FeaturesService = featuresService;

			WarehouseEntry = builder.ForProperty(x => x.Warehouse).UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}
	}
}
