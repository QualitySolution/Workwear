using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;

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

		private StockDocumentType? stokDocumentType;
		public virtual StockDocumentType? StockDocumentType {
			get => stokDocumentType;
			set => SetField(ref stokDocumentType, value);
		}

		public IEnumerable<object> HidenStockDocumentTypeList {
			get {
				if(!FeaturesService.Available(WorkwearFeature.CollectiveExpense))
					yield return Workwear.Domain.Stock.Documents.StockDocumentType.CollectiveExpense;
				if(!FeaturesService.Available(WorkwearFeature.Inspection))
					yield return Workwear.Domain.Stock.Documents.StockDocumentType.InspectionDoc;
				if(!FeaturesService.Available(WorkwearFeature.Completion))
					yield return Workwear.Domain.Stock.Documents.StockDocumentType.Completion;
				if(!FeaturesService.Available(WorkwearFeature.Warehouses))
					yield return Workwear.Domain.Stock.Documents.StockDocumentType.TransferDoc;
				if(!FeaturesService.Available(WorkwearFeature.Completion))
					yield return Workwear.Domain.Stock.Documents.StockDocumentType.Completion;
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

		public StockDocumentsFilterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			JournalViewModelBase journal,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			StockRepository stockRepository,
			FeaturesService featuresService): base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<StockDocumentsFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			FeaturesService = featuresService;

			if(VisibleWarehouse) //Заполняем склад только если он видимый. Так как иначе пользователю не возможно будет увидеть документы не относящиеся к складу, так как он не сможет очистить поле.
				warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			WarehouseEntry = builder.ForProperty(x => x.Warehouse).MakeByType().Finish();
		}
	}
}
