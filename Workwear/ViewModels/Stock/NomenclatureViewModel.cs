using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Communications;
using QS.Utilities;
using Workwear.Domain.Sizes;
using Workwear.Models.Sizes;

namespace Workwear.ViewModels.Stock
{
	public class NomenclatureViewModel : EntityDialogViewModelBase<Nomenclature> {
		private readonly FeaturesService featuresService;
		private readonly IInteractiveService interactive;
		private readonly ModalProgressCreator progressCreator;
		private readonly SizeTypeReplaceModel sizeTypeReplaceModel;

		public NomenclatureViewModel(
			BaseParameters baseParameters,
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope,
			FeaturesService featuresService,
			IInteractiveService interactive,
			ModalProgressCreator progressCreator,
			SizeTypeReplaceModel sizeTypeReplaceModel,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			this.sizeTypeReplaceModel = sizeTypeReplaceModel ?? throw new ArgumentNullException(nameof(sizeTypeReplaceModel));
			
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

			lastSizeType = Entity.Type?.SizeType;
			lastHeightType = Entity.Type?.HeightType;
		}
		#region EntityViewModels
		public EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
		#endregion
		#region Visible
		public bool VisibleClothesSex => true; //Поле стало в базе обязательным для всех номенклатур.

		public bool VisibleSaleCost => featuresService.Available(WorkwearFeature.Selling);
		public bool VisibleRating => Entity.Rating != null && featuresService.Available(WorkwearFeature.Ratings);
		public bool VisibleBarcode => featuresService.Available(WorkwearFeature.Barcodes);
		#endregion
		#region Sensitive
		public bool SensitiveOpenMovements => Entity.Id > 0;
		#endregion
		#region Data
		public string ClothesSexLabel => "Пол: ";
		public string RatingLabel => Entity.Rating?.ToString("F1");
		public string RatingButtonLabel => Entity.RatingCount != null ? NumberToTextRus.FormatCase(Entity.RatingCount.Value, "Посмотреть {0} отзыв", "Посмотреть {0} отзыва", "Посмотреть {0} отзывов") : String.Empty;
		#endregion
		#region Actions
		public void OpenMovements() {
			NavigationManager.OpenViewModel<StockMovmentsJournalViewModel>(this,
					addingRegistrations: builder => builder.RegisterInstance(Entity));
		}

		public void OpenRating() {
			var page = NavigationManager.OpenViewModel<RatingsViewModel, Nomenclature>(this, Entity);
		}
		#endregion
		
		private SizeType lastSizeType;
		private SizeType lastHeightType;
		public override bool Save() {
			if(!Validate())
				return false;
			//Обрабатываем размеры
			if(!UoW.IsNew && ((lastSizeType != null && !lastSizeType.IsSame(Entity.Type.SizeType)) || (lastHeightType != null && !lastHeightType.IsSame(Entity.Type.HeightType)))) {
				if(!sizeTypeReplaceModel.TryReplaceSizes(UoW, interactive, progressCreator, new []{Entity}, lastSizeType, Entity.Type.SizeType, lastHeightType, Entity.Type.HeightType))
					return false;
			}
			UoW.Save();
			lastSizeType = Entity.Type.SizeType;
			lastHeightType = Entity.Type.HeightType;
			return true;
		}
		private void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName != nameof(Entity.Type)) return;
			if (Entity.Type != null && String.IsNullOrWhiteSpace(Entity.Name))
				Entity.Name = Entity.Type.Name;

			OnPropertyChanged(nameof(VisibleClothesSex));
			OnPropertyChanged(nameof(ClothesSexLabel));
		}
	}
}
