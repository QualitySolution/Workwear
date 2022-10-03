using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock.Barcodes;

namespace Workwear.ViewModels.Stock 
{
	public class BarcodeViewModel : EntityDialogViewModelBase<Barcode>
	{
		public BarcodeViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}

		#region ViewProperty
		public bool EmployeeIssueVisible => Entity.EmployeeIssueOperation != null;
		public string EmployeeIssueTitle => Entity.EmployeeIssueOperation.Title + " " + Entity.Fractional;

		#endregion
	}
}
