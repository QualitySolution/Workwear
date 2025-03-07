using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Tools.Sizes;

namespace Workwear.Journal.Filter.ViewModels.Stock {
	public class BarcodeJournalFilterViewModel : JournalFilterViewModelBase<BarcodeJournalFilterViewModel> {
		public BarcodeJournalFilterViewModel(
			JournalViewModelBase journalViewModel,
			SizeService sizeService,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IUnitOfWorkFactory unitOfWorkFactory = null) 
			: base(journalViewModel, unitOfWorkFactory)
		{
			this.sizeService = sizeService;
			var builder = new CommonEEVMBuilderFactory<BarcodeJournalFilterViewModel>(journalViewModel, this, UoW, navigation, autofacScope);
			
			WarehouseEntry = builder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			NomenclatureEntry = builder.ForProperty(x => x.Nomenclature).MakeByType().Finish();
		}
		
		private readonly SizeService sizeService;
		public EntityEntryViewModel<Nomenclature> NomenclatureEntry;
		public EntityEntryViewModel<Warehouse> WarehouseEntry;
		public bool HasSize => Nomenclature?.Type?.SizeType != null;
		public bool HasHeight => Nomenclature?.Type?.HeightType != null;
		
		private Nomenclature nomenclature;
		[PropertyChangedAlso(nameof(Sizes))]
		[PropertyChangedAlso(nameof(Heights))]
		[PropertyChangedAlso(nameof(HasSize))]
		[PropertyChangedAlso(nameof(HasHeight))]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}
		
		private Size size;
		public virtual Size Size {
			get => size;
			set => SetField(ref size, value);
		}

		private Size height;
		public virtual Size Height {
			get => height;
			set => SetField(ref height, value);
		}
		
		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}
		
		public Size[] Sizes => 
			nomenclature is null ? 
				sizeService.GetSizeByCategory(UoW, CategorySizeType.Size, false, true).ToArray() : 
				nomenclature.Type?.SizeType is null ? new Size[]{} : 
					sizeService.GetSize(UoW, nomenclature?.Type?.SizeType, false, true)
						.ToArray();
		public Size[] Heights => 
			nomenclature is null ? 
				sizeService.GetSizeByCategory(UoW, CategorySizeType.Height, false, true).ToArray() : 
				nomenclature.Type?.HeightType is null ? new Size[]{} : 
					sizeService.GetSize(UoW, nomenclature?.Type?.HeightType, false, true).ToArray();

	}
}
