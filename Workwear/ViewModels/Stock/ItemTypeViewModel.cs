using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Measurements;
using Workwear.Models.Sizes;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Stock
{
	public class ItemTypeViewModel : EntityDialogViewModelBase<ItemsType>
	{
		private readonly IInteractiveService interactive;
		private readonly FeaturesService featuresService;
		private readonly ModalProgressCreator progressCreator;
		private readonly SizeTypeReplaceModel sizeTypeReplaceModel;
		public SizeService SizeService { get; }

		public ItemTypeViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IInteractiveService interactive,
			FeaturesService featuresService, 
			SizeService sizeService,
			ModalProgressCreator progressCreator,
			SizeTypeReplaceModel sizeTypeReplaceModel,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.progressCreator = progressCreator;
			this.sizeTypeReplaceModel = sizeTypeReplaceModel ?? throw new ArgumentNullException(nameof(sizeTypeReplaceModel));
			SizeService = sizeService;
			Entity.PropertyChanged += Entity_PropertyChanged;
			lastSizeType = Entity.SizeType;
			lastHeightType = Entity.HeightType;
		}

		private SizeType lastSizeType;
		private SizeType lastHeightType;

		#region Visible
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense) && Entity.Category == ItemTypeCategory.wear;
		public bool VisibleWearCategory => Entity.Category == ItemTypeCategory.wear;
		public bool VisibleSize => Entity.Category == ItemTypeCategory.wear;
		#endregion

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch(e.PropertyName) {
				case nameof(Entity.Category):
					OnPropertyChanged(nameof(VisibleWearCategory));
					OnPropertyChanged(nameof(VisibleIssueType));
					OnPropertyChanged(nameof(VisibleSize));
					break;
			}
		}

		public override bool Save() {
			if(!Validate())
				return false;
			//Обрабатываем размеры
			if(!UoW.IsNew && (lastSizeType != null || lastHeightType != null)) {
				if(!sizeTypeReplaceModel.TryReplaceSizes(UoW, interactive, progressCreator, Entity.Nomenclatures.ToArray(), lastSizeType, Entity.SizeType, lastHeightType, Entity.HeightType))
					return false;
			}
			UoW.Save();
			lastSizeType = Entity.SizeType;
			lastHeightType = Entity.HeightType;
			return true;
		}
	}
}
