using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace workwear.ViewModels.Operations 
{
	public class ManualEmployeeIssueOperationsViewModel : UowDialogViewModelBase, IWindowDialogSettings
	{
		public ManualEmployeeIssueOperationsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IValidator validator = null, 
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle)
		{
		}
		
		#region Windows Settings

		public bool IsModal { get; }
		public bool EnableMinimizeMaximize { get; }
		public bool Resizable { get; }
		public bool Deletable { get; }
		public WindowGravity WindowPosition { get; }
		
		#endregion
	}
}
