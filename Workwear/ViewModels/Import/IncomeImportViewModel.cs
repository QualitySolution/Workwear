using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace workwear.ViewModels.Import 
{
	public class IncomeImportViewModel : UowDialogViewModelBase
	{
		public IncomeImportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation) : base(unitOfWorkFactory, navigation)
		{
		}
	}
}
