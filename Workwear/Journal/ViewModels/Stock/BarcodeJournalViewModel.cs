using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Report;
using QS.Report.ViewModels;
using QS.Utilities.Text;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Tools;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock 
{
	public class BarcodeJournalViewModel : EntityJournalViewModelBase<Barcode, BarcodeViewModel, BarcodeJournalNode>, IDialogDocumentation {
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#barcodes");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(Barcode));
		#endregion
		
		private readonly BaseParameters baseParameters;
		
		public BarcodeJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager,
			BaseParameters baseParameters, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) 
		{
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			
			UseSlider = true;
			VisibleCreateAction = false;
			
			TableSelectionMode = JournalSelectionMode.Multiple;

			CreateFunctionPrintBarcodes();
		}

		protected override IQueryOver<Barcode> ItemsQuery(IUnitOfWork uow) 
		{
			BarcodeJournalNode resultAlias = null;
			
			Barcode barcodeAlias = null;
			Nomenclature nomenclatureAlias = null;
			EmployeeCard employeeAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			
			return  uow.Session.QueryOver<Barcode>(() => barcodeAlias)
				.Where(MakeSearchCriterion().By(
					() => barcodeAlias.Title,
					() => nomenclatureAlias.Name,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => barcodeAlias.Comment
				).WithLikeMode(MatchMode.Exact).By(
					() => employeeAlias.PersonnelNumber
				).Finish())
				.Left.JoinAlias(x => x.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(x => x.Size, () => sizeAlias)
				.Left.JoinAlias(x => x.Height, () => heightAlias)
				.Left.JoinAlias(x => x.BarcodeOperations, () => barcodeOperationAlias)
				.Left.JoinAlias(() => barcodeOperationAlias.EmployeeIssueOperation, () => employeeIssueOperationAlias)
				.Left.JoinAlias(() => employeeIssueOperationAlias.Employee, () => employeeAlias)
				.SelectList((list) => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Type).WithAlias(() => resultAlias.Type)
					.Select(x => x.Title).WithAlias(() => resultAlias.Value)
					.Select(x => x.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.Size)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
				).OrderBy(x => x.Id).Desc
				.TransformUsing(Transformers.AliasToBean<BarcodeJournalNode>());
		}
		
		#region Actions

		public void CreateFunctionPrintBarcodes() {

			NodeActionsList.Add(new JournalAction("Печать",
				(nodes) => nodes.Cast<BarcodeJournalNode>().Any(),
				(nodes) => nodes.Cast<BarcodeJournalNode>().Any(b => b.Type == BarcodeTypes.EAN13),
				PrintBarcodes));
		}

		#endregion

		public void PrintBarcodes(object[] nodes) {
			
			var reportInfo = new ReportInfo {
				Title = "Штрихкод",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> {
					{"barcodes", nodes.Cast<BarcodeJournalNode>().Select(x => x.Id).ToList()}
				}
			};
			
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
	}

	public class BarcodeJournalNode 
	{
		public int Id { get; set; }
		public BarcodeTypes Type { get; set; }
		public string Value { get; set; }
		public string Nomenclature { get; set; }
		public string Size { get; set; }
		public string Height { get; set; }
		public DateTime CreateDate { get; set; }
		public string Comment { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public string FullName => PersonHelper.PersonFullName(LastName, FirstName, Patronymic);
	}
}
