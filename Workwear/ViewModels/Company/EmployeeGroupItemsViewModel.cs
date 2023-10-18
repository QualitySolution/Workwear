using QS.Dialog;
using QS.Navigation;
using QS.ViewModels;


namespace Workwear.ViewModels.Company {
	public class EmployeeGroupItemsViewModel : ViewModelBase{
		public EmployeeGroupItemsViewModel(
			EmployeeGroupViewModel parent, 
			INavigationManager navigation) {
				this.parent = parent;
				this.navigation = navigation;
			}

		private readonly EmployeeGroupViewModel parent;
		private readonly INavigationManager navigation;
		
		public void SelectItem(int id) {
			throw new System.NotImplementedException();
		}

		public void OnShow() { }
	}
}
