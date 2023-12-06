using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;

namespace Workwear.ViewModels.Stock 
{
	public class BarcodeViewModel : EntityDialogViewModelBase<Barcode> {
		
		public BarcodeViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			
		}
		
		public void PrintBarcodes() {
			var reportInfo = new ReportInfo {
				Title = "Штрихкод",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> {
					{"barcodes", Entity.BarcodeOperations.Select(x => x.Barcode.Id).ToArray()}
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}

		#region ViewProperty


		#endregion
	}
}
