using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace workwear.ViewModels.Communications 
{
	public class RatingsViewModel : UowDialogViewModelBase 
	{
		public RatingsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IValidator validator = null, 
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle)
		{
		}
	}
}
