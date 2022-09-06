using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Communications;

namespace workwear.ViewModels.Stock
{
	public class NomenclatureViewModel : EntityDialogViewModelBase<Nomenclature> {
		private readonly FeaturesService featuresService;
		public NomenclatureViewModel(
			BaseParameters baseParameters,
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope,
			FeaturesService featuresService,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			var entryBuilder = 
				new CommonEEVMBuilderFactory<Nomenclature>(this, Entity, UoW, navigation, autofacScope);

			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
				.MakeByType()
				.Finish();
			Validations.Clear();
			Validations.Add(
				new ValidationRequest(Entity, 
					new ValidationContext(Entity, 
						new Dictionary<object, object> { { nameof(BaseParameters), baseParameters }, 
							{nameof(IUnitOfWork), UoW} })));

			Entity.PropertyChanged += Entity_PropertyChanged;

			this.featuresService = featuresService;
		}
		#region EntityViewModels
		public EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
		#endregion
		#region Visible
		public bool VisibleClothesSex =>
			Entity.Type != null && Entity.Type.Category == ItemTypeCategory.wear;

		public bool VisibleRating => Entity.Rating != null && featuresService.Available(WorkwearFeature.Claims);
		#endregion
		#region Sensitive
		public bool SensitiveOpenMovements => Entity.Id > 0;
		#endregion
		#region Data
		public string ClothesSexLabel => "Пол: ";
		public string RatingLabel => Entity.Rating != null ? Entity.Rating.Value.ToString("F") : String.Empty;

		#endregion
		#region Actions
		public void OpenMovements() {
			NavigationManager.OpenViewModel<StockMovmentsJournalViewModel>(this,
					addingRegistrations: builder => builder.RegisterInstance(Entity));
		}

		public void OpenRating() {
			var page = NavigationManager.OpenViewModel<RatingsViewModel, Nomenclature>(this, Entity, OpenPageOptions.AsSlave);
			page.ViewModel.EntryNomenclatureVisible = false;
		}
		#endregion
		private void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName != nameof(Entity.Type)) return;
			if (Entity.Type != null && String.IsNullOrWhiteSpace(Entity.Name))
				Entity.Name = Entity.Type.Name;

			OnPropertyChanged(nameof(VisibleClothesSex));
			OnPropertyChanged(nameof(ClothesSexLabel));
		}
	}
}
