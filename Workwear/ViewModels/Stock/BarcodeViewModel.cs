using System.ComponentModel;
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
			Entity.PropertyChanged += EntityOnPropertyChanged;
		}

		private void EntityOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName)
			{
				case nameof(Entity.EmployeeIssueOperation):
					OnPropertyChanged(nameof(EmployeeIssueVisible));
					OnPropertyChanged(nameof(EmployeeIssueTitle));
					OnPropertyChanged(nameof(OperationsTitle));
					break;
				case nameof(Entity.Fractional):
					OnPropertyChanged(nameof(EmployeeIssueTitle));
					break;
			} 
		}

		#region ViewProperty
		public bool EmployeeIssueVisible => Entity.EmployeeIssueOperation != null;
		public string EmployeeIssueTitle => Entity.EmployeeIssueOperation?.Title + " " + Entity.Fractional;
		public string OperationsTitle => Entity.EmployeeIssueOperation is null ? "Нет привязаных операций" : "Привязанные операции";

		#endregion

		#region Methods

		public void DeleteEmployeeIssue() => Entity.EmployeeIssueOperation = null;

		#endregion
	}
}
