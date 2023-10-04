using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;
using Workwear.Repository.Stock;
using Workwear.Tools.Sizes;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class StockMovementsFilterViewModel : JournalFilterViewModelBase<StockMovementsFilterViewModel>
	{
		private readonly FeaturesService featuresService;
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
		[PropertyChangedAlso(nameof(SensitiveSize))]
		[PropertyChangedAlso(nameof(SensitiveGrowth))]
		public virtual StockPosition StockPosition {
			get => stockPosition;
			set {
				if(SetField(ref stockPosition, value))
					EntryNomenclature.IsEditable = stockPosition == null;
			}
		}

		private Nomenclature nomenclature;
		[PropertyChangedAlso(nameof(Sizes))]
		[PropertyChangedAlso(nameof(Growths))]
		[PropertyChangedAlso(nameof(SensitiveSize))]
		[PropertyChangedAlso(nameof(SensitiveGrowth))]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		private Size size;
		public virtual Size Size {
			get => StockPosition is null ? size : StockPosition.WearSize;
			set => SetField(ref size, value);
		}

		private Size height;
		public virtual Size Height {
			get => StockPosition == null ? height : StockPosition.Height;
			set => SetField(ref height, value);
		}
		
		private Owner owner;
		public virtual Owner Owner {
			get => StockPosition is null ? owner : StockPosition.Owner;
			set => SetField(ref owner, value);
		}

		private bool collapseCollectiveIssue;
		public bool CollapseOperationItems {
			get => collapseCollectiveIssue;
			set => SetField(ref collapseCollectiveIssue, value);
		}
		#endregion
		public string StockPositionTitle => StockPosition?.Title;
		#region Visible
		public bool VisibleWarehouse => featuresService.Available(WorkwearFeature.Warehouses);
		public bool VisibleOwner => featuresService.Available(WorkwearFeature.Owners);
		#endregion
		
		#region Sensitive
		public bool SensitiveNomeclature => StockPosition == null;
		public bool SensitiveSize => StockPosition == null;
		public bool SensitiveGrowth => StockPosition == null;
		#endregion
		#region ComboValues
		public Size[] Sizes => 
			nomenclature is null ? 
				sizeService.GetSizeByCategory(UoW, CategorySizeType.Size, false, true).ToArray() : 
				nomenclature.Type?.SizeType is null ? new Size[]{} : 
					sizeService.GetSize(UoW, nomenclature?.Type?.SizeType, false, true)
						.ToArray();
		public Size[] Growths => 
			nomenclature is null ? 
				sizeService.GetSizeByCategory(UoW, CategorySizeType.Height, false, true).ToArray() : 
				nomenclature.Type?.HeightType is null ? new Size[]{} : 
					sizeService.GetSize(UoW, nomenclature?.Type?.HeightType, false, true).ToArray();

		public List<Owner> Owners {
			get {
				List<Owner> owners = new List<Owner>() {
					new Owner() { Id = -1, Name = "Все" }, 
					new Owner() { Id = 0, Name = "Без собственика" }
				}; 
				owners.AddRange(UoW.GetAll<Owner>());
				return owners;
			}
		}
		
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
			FeaturesService featuresService,
			StockRepository stockRepository,
			SizeService sizeService, Nomenclature nomenclature = null): base(journal, unitOfWorkFactory)
		{
			this.sizeService = sizeService;
			var builder = new CommonEEVMBuilderFactory<StockMovementsFilterViewModel>(journal, this, UoW, navigation, autofacScope);
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));

			if(nomenclature != null)
				this.nomenclature = UoW.GetById<Nomenclature>(nomenclature.Id);

			warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			WarehouseEntry = builder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			EntryNomenclature = builder.ForProperty(x => x.Nomenclature).MakeByType().Finish();
		}
	}
	public enum DirectionOfOperation {
		[Display(Name = "Поступление и расход")]
		all,
		[Display(Name = "Только поступление")]
		receipt,
		[Display(Name = "Только расход")]
		expense
	}
}
