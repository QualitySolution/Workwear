using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.ViewModels.Stock 
{
	public class BarcodeViewModel : EntityDialogViewModelBase<Barcode>, IDialogDocumentation {
		
		public BarcodeViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			LoadBarcodeOperations();
		}

		private void LoadBarcodeOperations() {
			if(Entity.Id == 0)
				return;

			BarcodeOperation barcodeOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			OverNormOperation overNormOperationAlias = null;

			UoW.Session.QueryOver<Barcode>()
				.Where(x => x.Id == Entity.Id)
				.Fetch(SelectMode.Fetch, x => x.BarcodeOperations)
				.Left.JoinAlias(x => x.BarcodeOperations, () => barcodeOperationAlias)
				.Fetch(SelectMode.Fetch, () => barcodeOperationAlias.EmployeeIssueOperation)
				.Fetch(SelectMode.Fetch, () => barcodeOperationAlias.WarehouseOperation)
				.Fetch(SelectMode.Fetch, () => barcodeOperationAlias.OverNormOperation)
				.Left.JoinAlias(() => barcodeOperationAlias.EmployeeIssueOperation, () => employeeIssueOperationAlias)
				.Fetch(SelectMode.Fetch, () => employeeIssueOperationAlias.Employee)
				.Left.JoinAlias(() => barcodeOperationAlias.OverNormOperation, () => overNormOperationAlias)
				.Fetch(SelectMode.Fetch, () => overNormOperationAlias.Employee)
				.List();
		}
		
		public void PrintBarcodes() {
			var reportInfo = new ReportInfo {
				Title = "Штрихкод",
				Identifier = "Barcodes.Barcode",
				Parameters = new Dictionary<string, object> {
					{"barcodes", Entity.Id}
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#barcodes");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion

		#region ViewProperty

		public bool CanPrint => Entity.Type == BarcodeTypes.EAN13;

		#endregion
	}
}
