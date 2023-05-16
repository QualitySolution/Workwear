using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Measurements;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Sizes
{
	public class SizeTypeViewModel : EntityDialogViewModelBase<SizeType>
	{
		private readonly FeaturesService featuresService;
		private readonly SizeService sizeService;
		public SizeTypeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			FeaturesService featuresService,
			SizeService sizeService,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.sizeService = sizeService;
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, 
				new ValidationContext(Entity, new Dictionary<object, object> {{nameof(IUnitOfWork), UoW} })));
			if (UoW.IsNew) {
				IsNew = true;
				Sizes = new GenericObservableList<Size>();
			}
			else {
				Sizes = new GenericObservableList<Size>(sizeService.GetSize(UoW, Entity).ToList());
				if (Entity.Id <= SizeService.MaxStandardSizeTypeId) IsStandardType = true;
			}
		}

		public bool IsNew { get; }
		public bool IsStandardType { get; }

		#region для View
		public bool CanAdd => featuresService.Available(WorkwearFeature.CustomSizes);
		public bool CanEdit => (IsNew || !IsStandardType) && featuresService.Available(WorkwearFeature.CustomSizes);
		#endregion
		private GenericObservableList<Size> sizes;
		public GenericObservableList<Size> Sizes {
			get => sizes;
			set => SetField(ref sizes, value);
		}
		public void AddSize() {
			var page = NavigationManager
				.OpenViewModel<SizeViewModel, IEntityUoWBuilder, SizeType>(
					this, EntityUoWBuilder.ForCreate(), Entity);
			page.PageClosed += (sender, args) => {
				sizeService.RefreshSizes(UoW);
				Sizes = new GenericObservableList<Size>(sizeService.GetSize(UoW, Entity).ToList());
			};
		}

		public void RemoveSize(Size size) {
			UoW.Delete(size);
			Sizes.Remove(size);
		}
		public void OpenSize(int sizeId) =>
			NavigationManager.OpenViewModel<SizeViewModel, IEntityUoWBuilder, SizeType>(
				this, EntityUoWBuilder.ForOpen(sizeId), Entity);
	}
}
