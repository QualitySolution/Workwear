using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.ViewModels.Extension;
using Workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.Journal.ViewModels.Stock {
	public class OverNormIssuedJournalViewModel : JournalViewModelBase, IDialogDocumentation {
		public OverNormIssuedJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILifetimeScope autofacScope
		) : base(unitOfWorkFactory, interactiveService, navigation) {
			JournalFilter = Filter = autofacScope.Resolve<OverNormIssuedJournalFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));
			DataLoader = new AnyDataLoader<OverNormIssuedJournalNode>(GetNodes);
			SelectionMode = JournalSelectionMode.Multiple;
			Title = "Выдано сверх нормы";
			UpdateOnChanges(typeof(OverNormOperation));
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#over-norm-issue");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip(Title);
		#endregion

		public OverNormIssuedJournalFilterViewModel Filter { get; }

		private EmployeeCard employee;
		public EmployeeCard Employee {
			get => employee;
			set {
				if(SetField(ref employee, value))
					DataLoader?.LoadData(false);
			}
		}

		private IList<OverNormIssuedJournalNode> GetNodes(CancellationToken cancellation) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				OverNormIssuedJournalNode resultAlias = null;
				OverNormOperation operationAlias = null;
				OverNormOperation returnedOperationAlias = null;
				WarehouseOperation warehouseOperationAlias = null;
				WarehouseOperation returnedWarehouseOperationAlias = null;
				EmployeeCard employeeAlias = null;
				Nomenclature nomenclatureAlias = null;
				Size sizeAlias = null;
				Size heightAlias = null;
				BarcodeOperation barcodeOperationAlias = null;
				Barcode barcodeAlias = null;

				var returnedOperationSubquery = QueryOver.Of(() => returnedOperationAlias)
					.JoinAlias(() => returnedOperationAlias.WarehouseOperation, () => returnedWarehouseOperationAlias)
					.Where(() => returnedOperationAlias.ReturnFromOperation.Id == operationAlias.Id)
					.Select(Projections.Sum(() => returnedWarehouseOperationAlias.Amount));

				var balanceProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Int32, "(?1 - IFNULL(?2, 0))"),
					NHibernateUtil.Int32,
					Projections.Property(() => warehouseOperationAlias.Amount),
					Projections.SubQuery(returnedOperationSubquery));

				var employeeNameProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
					NHibernateUtil.String,
					Projections.Property(() => employeeAlias.LastName),
					Projections.Property(() => employeeAlias.FirstName),
					Projections.Property(() => employeeAlias.Patronymic));

				var barcodesProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String,
						"GROUP_CONCAT(DISTINCT IF(NOT EXISTS (" +
						"SELECT 1 FROM operation_barcodes return_barcode_operation " +
						"LEFT JOIN operation_over_norm return_operation " +
							"ON return_operation.id = return_barcode_operation.over_norm_operation_id " +
						"WHERE return_operation.return_from_operation = ?1 " +
							"AND return_barcode_operation.barcode_id = ?2" +
						"), ?3, NULL) SEPARATOR '\n')"),
					NHibernateUtil.String,
					Projections.Property(() => operationAlias.Id),
					Projections.Property(() => barcodeAlias.Id),
					Projections.Property(() => barcodeAlias.Title));

				var query = uow.Session.QueryOver(() => operationAlias)
					.JoinAlias(() => operationAlias.WarehouseOperation, () => warehouseOperationAlias)
					.JoinAlias(() => operationAlias.Employee, () => employeeAlias)
					.JoinAlias(() => operationAlias.Nomenclature, () => nomenclatureAlias)
					.Left.JoinAlias(() => operationAlias.WearSize, () => sizeAlias)
					.Left.JoinAlias(() => operationAlias.Height, () => heightAlias)
					.Left.JoinAlias(() => operationAlias.BarcodeOperations, () => barcodeOperationAlias)
					.Left.JoinAlias(() => barcodeOperationAlias.Barcode, () => barcodeAlias)
					.Where(() => warehouseOperationAlias.ExpenseWarehouse != null)
					.Where(() => operationAlias.ReturnFromOperation == null)
					.Where(Restrictions.Gt(balanceProjection, 0))
					.Where(GetSearchCriterion(
						() => employeeAlias.LastName,
						() => employeeAlias.FirstName,
						() => employeeAlias.Patronymic,
						() => employeeAlias.PersonnelNumber,
						() => nomenclatureAlias.Name,
						() => barcodeAlias.Title
					));

				if(Employee != null)
					query.Where(() => employeeAlias.Id == Employee.Id);

				if(Filter.Type != null)
					query.Where(() => operationAlias.Type == Filter.Type);

				return query
					.SelectList(list => list
						.SelectGroup(() => operationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => operationAlias.Type).WithAlias(() => resultAlias.Type)
						.Select(employeeNameProjection).WithAlias(() => resultAlias.EmployeeName)
						.Select(() => employeeAlias.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
						.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
						.Select(balanceProjection).WithAlias(() => resultAlias.Amount)
						.Select(() => warehouseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => warehouseOperationAlias.OperationTime).WithAlias(() => resultAlias.Date)
						.Select(barcodesProjection).WithAlias(() => resultAlias.Barcodes)
					)
					.OrderBy(() => warehouseOperationAlias.OperationTime).Desc
					.ThenBy(() => operationAlias.Id).Desc
					.TransformUsing(Transformers.AliasToBean<OverNormIssuedJournalNode>())
					.List<OverNormIssuedJournalNode>();
			}
		}
	}

	public class OverNormIssuedJournalNode {
		public int Id { get; set; }
		public OverNormType Type { get; set; }
		public string EmployeeName { get; set; }
		public string PersonnelNumber { get; set; }
		public string NomenclatureName { get; set; }
		public string WearSize { get; set; }
		public string Height { get; set; }
		public int Amount { get; set; }
		public decimal WearPercent { get; set; }
		public DateTime Date { get; set; }
		public string Barcodes { get; set; }
		public string TypeText => Type.GetEnumTitle();
		public string DateText => Date.ToShortDateString();
		public string WearPercentText => WearPercent.ToString("P0");
	}
}
