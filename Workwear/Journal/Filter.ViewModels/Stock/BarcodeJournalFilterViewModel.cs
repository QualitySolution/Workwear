using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock;

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
			NomenclatureEntry = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclatureJournalViewModel, NomenclatureFilterViewModel>(f => f.OnlyMarking = true)
                .UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
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
			set {
				if(SetField(ref nomenclature, value) && Employee != null) {
					Size = Employee.Sizes.FirstOrDefault(s => s.SizeType.Id == value?.Type?.SizeType.Id)?.Size;
					Height = Employee.Sizes.FirstOrDefault(s => s.SizeType.Id == value?.Type?.HeightType.Id)?.Size;
				}
			}
		}

		private EmployeeCard employee;
		//Скрытый фильтр для автоподстановки разммеров
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
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
		
		private StockPosition stockPosition;
		/// <summary>
		/// конкреттные позиции на складе
		/// </summary>
		[PropertyChangedAlso(nameof(Nomenclature))]
		[PropertyChangedAlso(nameof(Size))]
		[PropertyChangedAlso(nameof(Height))]
		public virtual StockPosition StockPosition {
			get => stockPosition;
			set {
				SetField(ref stockPosition, value);
				Nomenclature = value.Nomenclature;
				Size = value.WearSize;
				Height = value.Height;
			}
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
		
		private bool onlyFreeBarcodes;
		public virtual bool OnlyFreeBarcodes {
			get => onlyFreeBarcodes;
			set => SetField(ref onlyFreeBarcodes, value);
		}
		
		/// <summary>
		/// All filters
		/// </summary>
		private bool canUseFilter = true;
		public virtual bool CanUseFilter {
			get => canUseFilter;
			set => SetField(ref canUseFilter, value);
		}
	}
}
