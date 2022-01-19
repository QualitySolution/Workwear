using System;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Stock;
using workwear.Tools.Features;
using Workwear.Measurements;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class StockMovementsFilterViewModel : JournalFilterViewModelBase<StockMovementsFilterViewModel>
	{
		public readonly FeaturesService FeaturesService;
		private readonly SizeService sizeService;

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
		[PropertyChangedAlso(nameof(SensitiveNomeclature))]
		public virtual StockPosition StockPosition {
			get => stockPosition;
			set {
				if(SetField(ref stockPosition, value))
					EntryNomenclature.IsEditable = stockPosition == null;
			}
		}

		private Nomenclature nomenclature;
		[PropertyChangedAlso(nameof(SensitiveSize))]
		[PropertyChangedAlso(nameof(SensitiveGrowth))]
		[PropertyChangedAlso(nameof(Sizes))]
		[PropertyChangedAlso(nameof(Growths))]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		private string size;
		public virtual string Size {
			get => size;
			set => SetField(ref size, value);
		}

		private string growth;
		public virtual string Growth {
			get => growth;
			set => SetField(ref growth, value);
		}

		private bool collapseCollectiveIssue;
		public bool CollapseOperationItems {
			get => collapseCollectiveIssue;
			set => SetField(ref collapseCollectiveIssue, value);
		}
		#endregion

		public string StockPositionTitle => StockPosition?.Title;

		#region Visible
		public bool VisibleWarehouse => FeaturesService.Available(Tools.Features.WorkwearFeature.Warehouses);
		#endregion

		#region Sensitive
		public bool SensitiveNomeclature => StockPosition == null;
		public bool SensitiveSize => sizeService.HasSize(Nomenclature?.Type?.WearCategory);
		public bool SensitiveGrowth => sizeService.HasGrowth(Nomenclature?.Type?.WearCategory);
		#endregion

		#region ComboValues
		public string[] Sizes => sizeService.GetSizesForNomeclature(Nomenclature?.SizeStd);
		public string[] Growths => sizeService.GetGrowthForNomenclature();

		private DirectionOfOperation direction;
		public DirectionOfOperation Direction {
			get => direction;
			set => SetField(ref direction, value);
		}
		#endregion

		public EntityEntryViewModel<Nomenclature> EntryNomenclature;
		public EntityEntryViewModel<Warehouse> WarehouseEntry;

		public StockMovementsFilterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			JournalViewModelBase journal,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			SizeService sizeService,
			FeaturesService featuresService, 
			Nomenclature nomenclature = null): base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<StockMovementsFilterViewModel>(journal, this, UoW, navigation, autofacScope);
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));

			if(nomenclature != null)
				this.nomenclature = UoW.GetById<Nomenclature>(nomenclature.Id);

			WarehouseEntry = builder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			EntryNomenclature = builder.ForProperty(x => x.Nomenclature).MakeByType().Finish();
		}
	}

	public enum DirectionOfOperation {
		[Display(Name = "поступление и расход")]
		all,
		[Display(Name = "только поступление")]
		receipt,
		[Display(Name = "только расход")]
		expense
	}
}