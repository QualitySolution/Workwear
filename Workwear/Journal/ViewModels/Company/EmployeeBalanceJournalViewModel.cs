using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Measurement.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;
using workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class EmployeeBalanceJournalViewModel : JournalViewModelBase
	{
		public EmployeeBalanceFilterViewModel Filter { get; set; }
		public EmployeeBalanceJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigation,
            ILifetimeScope autofacScope,
            FeaturesService featuresService,
            EmployeeCard employeeCard = null
            ) : base(unitOfWorkFactory, interactiveService, navigation)
        {
	        FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
	        var dataLoader = new ThreadDataLoader<EmployeeBalanceJournalNode>(unitOfWorkFactory);
	        dataLoader.AddQuery(ItemsQuery);
	        DataLoader = dataLoader;
	        JournalFilter = Filter = autofacScope.Resolve<EmployeeBalanceFilterViewModel>(
		        new TypedParameter(typeof(JournalViewModelBase), this),
		        new TypedParameter(typeof(EmployeeCard), employeeCard)
		        );
	        
	        Title = employeeCard != null 
		        ? $"Числится за сотрудником - {Filter.Employee.Title}" 
		        : "Остатки по сотрудникам";
	        //Журнал используется только для выбора. Если понадобится другой вариант, передавайте режим через конструктор.
	        SelectionMode = JournalSelectionMode.Multiple;
        }

        public FeaturesService FeaturesService { get; }

        #region Query
        public IQueryOver<EmployeeIssueOperation> ItemsQuery(IUnitOfWork unitOfWork)
        {
	        EmployeeBalanceJournalNode resultAlias = null;
			EmployeeIssueOperation expenseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType nomenclatureItemTypesAlias = null;
			MeasurementUnit nomenclatureUnitsAlias = null;
			EmployeeIssueOperation removeOperationAlias = null;
			ProtectionTools protectionToolsAlias = null;
			ItemsType protectionToolsItemTypesAlias = null;
			MeasurementUnit protectionToolsUnitsAlias  = null;
			WarehouseOperation warehouseOperationAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			EmployeeCard employeeCardAlias = null;
			BarcodeOperation barcodeOperationAlias = null;
			Barcode barcodeAlias = null;

			var query = unitOfWork.Session.QueryOver(() => expenseOperationAlias);

			if (Filter.Employee != null)
				query.Where(e => e.Employee == Filter.Employee);
			if(Filter.Subdivision != null)
				query.Where(() => employeeCardAlias.Subdivision.Id == Filter.Subdivision.Id);
			if(!Filter.CheckShowAll)
				query.Where(e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > Filter.Date);
			
			var subQueryRemove = QueryOver.Of(() => removeOperationAlias)
				.Where(() => removeOperationAlias.IssuedOperation.Id == expenseOperationAlias.Id)
				.Select(Projections.Sum<EmployeeIssueOperation>(o => o.Returned));

			var balance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.Property(() => expenseOperationAlias.Issued),
				Projections.SubQuery(subQueryRemove)
			);

			var barcodesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String,
					"GROUP_CONCAT(DISTINCT IF(NOT EXISTS (" +
					"SELECT 1 FROM operation_barcodes return_barcode_operation " +
					"LEFT JOIN operation_issued_by_employee return_operation " +
						"ON return_operation.id = return_barcode_operation.employee_issue_operation_id " +
					"WHERE return_operation.issued_operation_id = ?1 " +
						"AND return_barcode_operation.barcode_id = ?2" +
					"), ?3, NULL) SEPARATOR '\n')"),
				NHibernateUtil.String,
				Projections.Property(() => expenseOperationAlias.Id),
				Projections.Property(() => barcodeAlias.Id),
				Projections.Property(() => barcodeAlias.Title));

			if (Filter.Employee != null)
				query
					.JoinAlias(() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureAlias.Type, () => nomenclatureItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureItemTypesAlias.Units, () => nomenclatureUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.ProtectionTools, () => protectionToolsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsAlias.Type, () => protectionToolsItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsItemTypesAlias.Units, () => protectionToolsUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Employee, () => employeeCardAlias)
					.Left.JoinAlias(() => expenseOperationAlias.BarcodeOperations, () => barcodeOperationAlias)
					.Left.JoinAlias(() => barcodeOperationAlias.Barcode, () => barcodeAlias)
					.Where(GetSearchCriterion(
						() => employeeCardAlias.LastName,
						() => employeeCardAlias.FirstName,
						() => employeeCardAlias.Patronymic,
						() => nomenclatureAlias.Name,
						() => protectionToolsAlias.Name,
						() => sizeAlias.Name,
						() => heightAlias.Name,
						() => barcodeAlias.Title))
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.SelectList(list => list
						.SelectGroup(() => expenseOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => nomenclatureUnitsAlias.Name).WithAlias(() => resultAlias.NomenclatureUnitsName)
						.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
						.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
						.Select(() => warehouseOperationAlias.Cost).WithAlias(() => resultAlias.AvgCost)
						.Select(() => expenseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => expenseOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
						.Select(() => expenseOperationAlias.ExpiryByNorm).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => expenseOperationAlias.AutoWriteoffDate).WithAlias(() => resultAlias.AutoWriteoffDate)
						.Select(() => employeeCardAlias.FirstName).WithAlias(() => resultAlias.FirstName)
						.Select(() => employeeCardAlias.LastName).WithAlias(() => resultAlias.LastName)
						.Select(() => employeeCardAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
						.Select(() => protectionToolsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsName)
						.Select(() => protectionToolsUnitsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsUnitsName)
						.Select(barcodesProjection).WithAlias(() => resultAlias.Barcodes)
						.Select(balance).WithAlias(() => resultAlias.Balance));
			else {
				query
					.JoinAlias(() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureAlias.Type, () => nomenclatureItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureItemTypesAlias.Units, () => nomenclatureUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.ProtectionTools, () => protectionToolsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsAlias.Type, () => protectionToolsItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsItemTypesAlias.Units, () => protectionToolsUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Employee, () => employeeCardAlias)
					.Left.JoinAlias(() => expenseOperationAlias.BarcodeOperations, () => barcodeOperationAlias)
					.Left.JoinAlias(() => barcodeOperationAlias.Barcode, () => barcodeAlias)
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.Where(GetSearchCriterion(
						() => employeeCardAlias.LastName,
						() => employeeCardAlias.FirstName,
						() => employeeCardAlias.Patronymic,
						() => nomenclatureAlias.Name,
						() => protectionToolsAlias.Name,
						() => sizeAlias.Name,
						() => heightAlias.Name,
						() => barcodeAlias.Title))
					.SelectList(list => list
						.Select(() => expenseOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => nomenclatureUnitsAlias.Name).WithAlias(() => resultAlias.NomenclatureUnitsName)
						.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
						.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
						.Select(() => warehouseOperationAlias.Cost).WithAlias(() => resultAlias.AvgCost)
						.Select(() => expenseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => expenseOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
						.Select(() => expenseOperationAlias.ExpiryByNorm).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => expenseOperationAlias.AutoWriteoffDate).WithAlias(() => resultAlias.AutoWriteoffDate)
						.Select(() => expenseOperationAlias.FixedOperation).WithAlias(() => resultAlias.FixedOperation)
						.Select(() => employeeCardAlias.FirstName).WithAlias(() => resultAlias.FirstName)
						.Select(() => employeeCardAlias.LastName).WithAlias(() => resultAlias.LastName)
						.Select(() => employeeCardAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
						.Select(() => protectionToolsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsName)
						.Select(() => protectionToolsUnitsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsUnitsName)
						.Select(barcodesProjection).WithAlias(() => resultAlias.Barcodes)
						.Select(balance).WithAlias(() => resultAlias.Balance));
				query = query.OrderBy(() => employeeCardAlias.LastName).Asc
					.ThenBy(() => employeeCardAlias.FirstName).Asc
					.ThenBy(() => employeeCardAlias.Patronymic).Asc;
			}
			return query.TransformUsing(Transformers.AliasToBean<EmployeeBalanceJournalNode>());
        }
        #endregion
	}

    public class EmployeeBalanceJournalNode
    {
	    public int Id { get; set; }
	    public string NomenclatureName { get; set;}
	    public string ProtectionToolsName { get; set; }
	    public string ItemName => NomenclatureName ?? ProtectionToolsName;
	    public string NomenclatureUnitsName { get; set;}
	    public string ProtectionToolsUnitsName { get; set;}
	    public string UnitsName => NomenclatureUnitsName ?? ProtectionToolsUnitsName;
	    public string WearSize { get; set; }
	    public string Height { get; set; }
	    public decimal AvgCost { get; set;}
	    public decimal WearPercent { get; set;}
	    public DateTime IssuedDate { get; set;}
	    public DateTime? StartUseDate { get; set; }
	    public DateTime? ExpiryDate { get; set;}
	    public DateTime? AutoWriteoffDate {get; set;}
	    public decimal Percentage => ExpiryDate != null ? EmployeeIssueOperation.CalculatePercentWear(DateTime.Today, StartUseDate, ExpiryDate, WearPercent) : 0;
	    public bool FixedOperation { get; set; }
	    public int Balance { get; set;}
	    public string BalanceText => $"{Balance} {UnitsName}";
	    public string Barcodes { get; set; }
	    public string AvgCostText => AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
	    public string EmployeeName => String.Join(" ", LastName, FirstName, Patronymic);
	    public string LastName { get; set; }
	    public string FirstName { get; set; }
	    public string Patronymic { get; set; }
    }
}
