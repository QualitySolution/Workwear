using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Communications 
{
	public class RatingsViewModel : UowDialogViewModelBase 
	{
		public RatingsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			ILifetimeScope autofacScope, 
			IValidator validator = null, 
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle)
		{
			var builder = new CommonEEVMBuilderFactory<RatingsViewModel>(
				this, this, UoW, NavigationManager, autofacScope);

			EntryNomenclature = builder.ForProperty(x => x.SelectNomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclatureJournalViewModel>()
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
		}

		#region Свойства

		private Nomenclature selectNomenclature;
		public Nomenclature SelectNomenclature {
			get => selectNomenclature;
			set => SetField(ref selectNomenclature, value);
		}

		#endregion

		#region Entry

		public EntityEntryViewModel<Nomenclature> EntryNomenclature;

		#endregion
	}
}
