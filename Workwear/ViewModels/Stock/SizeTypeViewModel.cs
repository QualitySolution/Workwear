using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Sizes;

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
		}
	}
}
