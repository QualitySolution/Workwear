using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;

namespace Workwear.ViewModels.Stock 
{
	public class OwnerViewModel : EntityDialogViewModelBase<Owner>
	{
		public OwnerViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
	}
}
