using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Tools.Barcodes;
using Workwear.ViewModels.Stock;
using Size = Workwear.Domain.Sizes.Size;

namespace Workwear.Journal.ViewModels.Stock 
{
	public class BarcodeJournalViewModel : EntityJournalViewModelBase<Barcode, BarcodeViewModel, BarcodeJournalNode>
	{
		private readonly BarcodeService barcodeService;
		public BarcodeJournalFilterViewModel Filter { get; private set; }
		public BarcodeJournalViewModel(
			BarcodeService barcodeService,
			ILifetimeScope autofacScope, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) 
		{
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			UseSlider = true;
			VisibleCreateAction = false;
			
			JournalFilter = Filter = autofacScope.Resolve<BarcodeJournalFilterViewModel>
				(new TypedParameter(typeof(JournalViewModelBase), this));
			
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
			WarehouseOperation warehouseOperationAlias = null;
			OverNormOperation overNormOperationAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			
			var query = uow.Session.QueryOver<Barcode>(() => barcodeAlias)
				.Where(GetSearchCriterion(
					() => barcodeAlias.Title,
					() => barcodeAlias.Label,
					() => nomenclatureAlias.Name,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => barcodeAlias.Comment
				));

				if(Filter.Nomenclature != null)
					query.Where(x => x.Nomenclature.Id == Filter.Nomenclature.Id);
				if(Filter.Size != null)
					query.Where(x => x.Size.Id == Filter.Size.Id);
				if(Filter.Height != null)
					query.Where(x => x.Height.Id == Filter.Height.Id);
				if(Filter.Warehouse != null) {
					BarcodeOperation barcodeOperationSubAlias = null;
					EmployeeIssueOperation empIsOperationSubAlias = null;
					WarehouseOperation whOperationSubAlias = null;
					OverNormOperation overNormOperationSubAlias = null;

					var subQuery = QueryOver.Of(() => barcodeOperationSubAlias)
						.Left.JoinAlias(() => barcodeOperationSubAlias.WarehouseOperation, () => whOperationSubAlias)
						.Left.JoinAlias(() => barcodeOperationSubAlias.OverNormOperation, () => overNormOperationSubAlias)
						.Left.JoinAlias(() => barcodeOperationSubAlias.EmployeeIssueOperation, () => empIsOperationSubAlias)
						.Where(() => barcodeOperationSubAlias.Barcode.Id == barcodeAlias.Id)
						.OrderBy(Projections.SqlFunction("coalesce", NHibernateUtil.Date,
                        	Projections.Property(() => whOperationSubAlias.OperationTime),
                        	Projections.Property(() => empIsOperationSubAlias.OperationTime),
                        	Projections.Property(() => overNormOperationSubAlias.OperationTime)))
                        	.Desc
                        .Select(x => whOperationSubAlias.ReceiptWarehouse.Id)
                        .Take(1);
					query.Where(Restrictions.Eq(Projections.SubQuery(subQuery), Filter.Warehouse.Id));
				}

				if(Filter.StockPosition != null){
					query.Where(() => warehouseOperationAlias.WearPercent == Filter.StockPosition.WearPercent);
					if(Filter.StockPosition.Owner != null)
						query.Where(() => warehouseOperationAlias.Owner.Id == Filter.StockPosition.Owner.Id);
					else
						query.Where(() => warehouseOperationAlias.Owner == null);
			}

			query.Left.JoinAlias(x => x.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(x => x.Size, () => sizeAlias)
				.Left.JoinAlias(x => x.Height, () => heightAlias)
				.Left.JoinAlias(x => x.BarcodeOperations, () => barcodeOperationAlias)
				.Left.JoinAlias(() => barcodeOperationAlias.EmployeeIssueOperation, () => employeeIssueOperationAlias)
				.Left.JoinAlias(() => employeeIssueOperationAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => barcodeOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => barcodeOperationAlias.OverNormOperation, () => overNormOperationAlias)
				.SelectList((list) => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Title).WithAlias(() => resultAlias.Value)
					.Select(x => x.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(x => x.Label).WithAlias(() => resultAlias.Label)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.Size)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
				).OrderBy(x => x.Title).Asc
				.TransformUsing(Transformers.AliasToBean<BarcodeJournalNode>());
			
			return query;
		}
		
		#region Constraint

		private Nomenclature nomenclature;
		public Nomenclature Nomenclature 
		{
			get => nomenclature;
			set 
			{
				SetField(ref nomenclature, value);
				DataLoader.LoadData(false);
			}
		}

		private Size size;
		public Size Size 
		{
			get => size;
			set 
			{
				SetField(ref size, value);
				DataLoader.LoadData(false);
			}
		}

		private Size height;
		public Size Height 
		{
			get => height;
			set 
			{
				SetField(ref height, value);
				DataLoader.LoadData(false);
			}
		}

		private Warehouse warehouse;
		public Warehouse Warehouse 
		{
			get => warehouse;
			set 
			{
				SetField(ref warehouse, value); 
				DataLoader.LoadData(false);
			}
		}

		#endregion
		
		#region Actions

		public void CreateFunctionPrintBarcodes() {

			NodeActionsList.Add(new JournalAction("Печать",
				(nodes) => nodes.Cast<BarcodeJournalNode>().Any(),
				(arg) => true,
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
		public string Value { get; set; }
		public string Nomenclature { get; set; }
		public string Size { get; set; }
		public string Height { get; set; }
		public DateTime CreateDate { get; set; }
		public string Label { get; set; }
		public string Comment { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public string FullName => PersonHelper.PersonFullName(LastName, FirstName, Patronymic);
	}
}
