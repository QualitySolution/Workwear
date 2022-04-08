using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Sizes;
using Workwear.Measurements;

namespace workwear.ViewModels.Stock
{
	public class SizeTypeViewModel : EntityDialogViewModelBase<SizeType>
	{
		public SizeTypeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, 
				new ValidationContext(Entity, new Dictionary<object, object> {{nameof(IUnitOfWork), UoW} })));
			if (UoW.IsNew) {
				IsNew = true;
				isStandartSize = true;
			}
			else {
				Sizes = new GenericObservableList<Size>(SizeService.GetSize(UoW, Entity).ToList());
				if (Entity.Id < 100) isStandartSize = true;
			}
		}

		public bool IsNew { get; }
		private readonly bool isStandartSize;
		public bool CanEdit => !IsNew && !isStandartSize;
		private GenericObservableList<Size> sizes;
		public GenericObservableList<Size> Sizes {
			get => sizes;
			set => SetField(ref sizes, value);
		}
		public void AddSize() {
			var page = NavigationManager
				.OpenViewModel<SizeViewModel, IEntityUoWBuilder, SizeType>(
					this, EntityUoWBuilder.ForCreate(), Entity);
			page.PageClosed += (sender, args) => 
				Sizes = new GenericObservableList<Size>(SizeService.GetSize(UoW, Entity).ToList());
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
