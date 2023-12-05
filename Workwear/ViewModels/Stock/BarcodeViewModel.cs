using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Data;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;

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
				Title = "Штрихкоды",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> {
					{"operations", Entity.BarcodeOperations.Select(x => x.EmployeeIssueOperation.Id).ToArray()}
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
		
		
		#region ViewProperty


		#endregion
	}
}
