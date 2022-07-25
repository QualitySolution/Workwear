using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace workwear.ViewModels.Communications 
{
	public class ClaimsViewModel : UowDialogViewModelBase {
		public ClaimsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IValidator validator = null, 
			string UoWTitle = "Обращения сотрудников") : base(unitOfWorkFactory, navigation, validator, UoWTitle) 
		{
		}
	}
}
