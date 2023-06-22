using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;

namespace workwear.Journal.Filter.ViewModels.Stock
{
	public class NomenclatureFilterViewModel : JournalFilterViewModelBase<NomenclatureFilterViewModel>
	{
		public EntityEntryViewModel<ItemsType> EntryItemsType;
		public EntityEntryViewModel<ProtectionTools> EntryProtectionTools;
		private NomenclatureJournalViewModel NomenclatureJournalViewModel { get; }

		public NomenclatureFilterViewModel(
			NomenclatureJournalViewModel nomenclatureJournalViewModel, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IUnitOfWorkFactory unitOfWorkFactory = null
			) : base(nomenclatureJournalViewModel, unitOfWorkFactory)
		{
			NomenclatureJournalViewModel = nomenclatureJournalViewModel;
			var builder = new CommonEEVMBuilderFactory<NomenclatureFilterViewModel>(nomenclatureJournalViewModel, this, UoW, navigation, autofacScope);

			EntryItemsType = builder.ForProperty(x => x.ItemType).MakeByType().Finish();
			EntryProtectionTools = builder.ForProperty(x => x.ProtectionTools).MakeByType().Finish();
		}

		#region Visible
		public bool OnlyWithRatingVisible => NomenclatureJournalViewModel.FeaturesService.Available(WorkwearFeature.Ratings);
		#endregion
		
		#region Ограничения
		private ItemsType itemType;
		public virtual ItemsType ItemType {
			get => itemType;
			set => SetField(ref itemType, value);
		}

		private ProtectionTools protectionTools;
		public ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		private bool showArchival;
		public bool ShowArchival {
			get => showArchival;
			set => SetField(ref showArchival, value);
		}

		private bool onlyWithRating;
		public bool OnlyWithRating {
			get => onlyWithRating;
			set => SetField(ref onlyWithRating, value);
		}

		#endregion
	}
}
