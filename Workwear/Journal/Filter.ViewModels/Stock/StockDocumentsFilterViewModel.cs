using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Stock;
using workwear.Tools.Features;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class StockDocumentsFilterViewModel : JournalFilterViewModelBase<StockDocumentsFilterViewModel>
	{
		#region Ограничения
		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}

		private StokDocumentType? stokDocumentType;
		public virtual StokDocumentType? StokDocumentType {
			get => stokDocumentType;
			set => SetField(ref stokDocumentType, value);
		}

		public IEnumerable<object> HidenStokDocumentTypeList {
			get {
				if(!FeaturesService.Available(WorkwearFeature.CollectiveExpense))
					yield return Domain.Stock.StokDocumentType.CollectiveExpense;
				if(!FeaturesService.Available(WorkwearFeature.Warehouses))
					yield return Domain.Stock.StokDocumentType.TransferDoc;
			}
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
		#endregion

		public readonly FeaturesService FeaturesService;

		#region Visible

		public bool VisibleWarehouse => FeaturesService.Available(WorkwearFeature.Warehouses);

		#endregion

		public EntityEntryViewModel<Warehouse> WarehouseEntry;

		public StockDocumentsFilterViewModel(IUnitOfWorkFactory unitOfWorkFactory, JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, FeaturesService featuresService): base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<StockDocumentsFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			FeaturesService = featuresService;

			WarehouseEntry = builder.ForProperty(x => x.Warehouse).MakeByType().Finish();
		}
	}
}
