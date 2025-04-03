using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Sizes;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Stock
{
	public class ItemTypeViewModel : EntityDialogViewModelBase<ItemsType>, IDialogDocumentation
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
			lastSizeType = Entity.SizeType;
			lastHeightType = Entity.HeightType;
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#items-type");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		private SizeType lastSizeType;
		private SizeType lastHeightType;

		#region Visible
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		public bool VisibleWearCategory => true;
		public bool VisibleSize => true;
		#endregion

		public override bool Save() {
			if(!Validate())
				return false;
			//Обрабатываем размеры
			if(!UoW.IsNew && ((lastSizeType != null && !lastSizeType.IsSame(Entity.SizeType)) || (lastHeightType != null && !lastHeightType.IsSame(Entity.HeightType)))) {
				if(!sizeTypeReplaceModel.TryReplaceSizes(UoW, interactive, progressCreator, Entity.Nomenclatures.ToArray(), Entity.SizeType, Entity.HeightType))
					return false;
			}
			UoW.Save();
			lastSizeType = Entity.SizeType;
			lastHeightType = Entity.HeightType;
			return true;
		}
	}
}
