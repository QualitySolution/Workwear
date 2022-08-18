using System;
using Autofac;
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
            EmployeeCard employeeCard = null) : base(unitOfWorkFactory, interactiveService, navigation)
        {
	        var dataLoader = new ThreadDataLoader<EmployeeBalanceJournalNode>(unitOfWorkFactory);
	        dataLoader.AddQuery(ItemsQuery);
	        DataLoader = dataLoader;
	        AutofacScope = autofacScope;
	        JournalFilter = Filter = AutofacScope.Resolve<EmployeeBalanceFilterViewModel>(
		        new TypedParameter(typeof(JournalViewModelBase), this));
	        this.Filter.Employee = employeeCard;
	        Title = employeeCard != null 
		        ? $"Числится за сотрудником - {Filter.Employee.Title}" 
		        : "Остатки по сотрудникам";
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
			EmployeeCard employeeCardAlias = null;

			var query = unitOfWork.Session.QueryOver(() => expenseOperationAlias);

			if (Filter.Employee != null)
				query.Where(e => e.Employee == Filter.Employee);

			var subQueryRemove = QueryOver.Of(() => removeOperationAlias)
				.Where(() => removeOperationAlias.IssuedOperation.Id == expenseOperationAlias.Id)
				.Select(Projections.Sum<EmployeeIssueOperation>(o => o.Returned));

			var balance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.Property(() => expenseOperationAlias.Issued),
				Projections.SubQuery(subQueryRemove)
			);
			if (Filter.Employee != null)
				query
					.JoinAlias(() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias)
					.JoinAlias(() => expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias)
					.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias)
					.JoinAlias(() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias,
						JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Employee, () => employeeCardAlias)
					.Where(e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > Filter.Date)
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.SelectList(list => list
						.SelectGroup(() => expenseOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
						.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
						.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
						.Select(() => warehouseOperationAlias.Cost).WithAlias(() => resultAlias.AvgCost)
						.Select(() => expenseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => expenseOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
						.Select(() => expenseOperationAlias.ExpiryByNorm).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => employeeCardAlias.FirstName).WithAlias(() => resultAlias.FirstName)
						.Select(() => employeeCardAlias.LastName).WithAlias(() => resultAlias.LastName)
						.Select(() => employeeCardAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
						.Select(balance).WithAlias(() => resultAlias.Balance));
			else {
				query
					.JoinAlias(() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias)
					.JoinAlias(() => expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias)
					.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias)
					.JoinAlias(() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias,
						JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Employee, () => employeeCardAlias)
					.Where(e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > Filter.Date)
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.SelectList(list => list
						.Select(() => expenseOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
						.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
						.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
						.Select(() => warehouseOperationAlias.Cost).WithAlias(() => resultAlias.AvgCost)
						.Select(() => expenseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => expenseOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
						.Select(() => expenseOperationAlias.ExpiryByNorm).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => employeeCardAlias.FirstName).WithAlias(() => resultAlias.FirstName)
						.Select(() => employeeCardAlias.LastName).WithAlias(() => resultAlias.LastName)
						.Select(() => employeeCardAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
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
	    public string EmployeeName => String.Join(" ", LastName, FirstName, Patronymic);
	    public string LastName { get; set; }
	    public string FirstName { get; set; }
	    public string Patronymic { get; set; }
    }
}