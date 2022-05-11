using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;

namespace workwear.Journal.ViewModels.Company
{
    public class EmployeeBalanceJournalViewModel : JournalViewModelBase
    {
	    public EmployeeCard Employee { get;}
	    public DateTime OnDate { get; }
	    
        public EmployeeBalanceJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigation,
            EmployeeCard employee,
            DateTime onDate) : base(unitOfWorkFactory, interactiveService, navigation)
        {
	        Employee = employee;
	        OnDate = onDate;
	        var dataLoader = new ThreadDataLoader<EmployeeBalanceJournalNode>(unitOfWorkFactory);
	        dataLoader.AddQuery(ItemsQuery);
	        DataLoader = dataLoader;
	        Title = $"Числится за сотрудником - {Employee.Title}";
        }

        #region Query
        public IQueryOver<EmployeeIssueOperation> ItemsQuery(IUnitOfWork unitOfWork)
        {
	        EmployeeBalanceJournalNode resultAlias = null;
			EmployeeIssueOperation expenseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			EmployeeIssueOperation removeOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;

			var query = unitOfWork.Session.QueryOver(() => expenseOperationAlias)
				.Where(e => e.Employee == Employee);

			var subQueryRemove = QueryOver.Of(() => removeOperationAlias)
				.Where(() => removeOperationAlias.IssuedOperation.Id == expenseOperationAlias.Id)
				.Select(Projections.Sum<EmployeeIssueOperation>(o => o.Returned));

			var balance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.Property(() => expenseOperationAlias.Issued),
				Projections.SubQuery(subQueryRemove)
			);

			return query
				.JoinAlias (() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(()=> expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias (() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias (() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias (() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
				.Where(e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > OnDate)
				.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
				.SelectList (list => list
					.SelectGroup (() => expenseOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitsName)
					.Select (() => sizeAlias.Name).WithAlias (() => resultAlias.WearSize)
					.Select (() => heightAlias.Name).WithAlias (() => resultAlias.Height)
					.Select (() => warehouseOperationAlias.Cost).WithAlias (() => resultAlias.AvgCost)
					.Select (() => expenseOperationAlias.WearPercent).WithAlias (() => resultAlias.WearPercent)
					.Select (() => expenseOperationAlias.OperationTime).WithAlias (() => resultAlias.IssuedDate)
					.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
					.Select (() => expenseOperationAlias.ExpiryByNorm).WithAlias (() => resultAlias.ExpiryDate)
					.Select(balance).WithAlias(() => resultAlias.Balance)
				)
				.TransformUsing(Transformers.AliasToBean<EmployeeBalanceJournalNode>());
        }
        #endregion
    }

    public class EmployeeBalanceJournalNode
    {
	    public int Id { get; set; }
	    public string NomenclatureName { get; set;}
	    public string UnitsName { get; set;}
	    public string WearSize { get; set; }
	    public string Height { get; set; }
	    public decimal AvgCost { get; set;}
	    public decimal WearPercent { get; set;}
	    public DateTime IssuedDate { get; set;}
	    public DateTime? StartUseDate { get; set; }
	    public DateTime? ExpiryDate { get; set;}
	    public decimal Percentage => 
		    EmployeeIssueOperation.CalculatePercentWear(DateTime.Today, StartUseDate, ExpiryDate, WearPercent);
	    public int Balance { get; set;}
	    public string BalanceText => $"{Balance} {UnitsName}";
	    public string AvgCostText => AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
    }
}