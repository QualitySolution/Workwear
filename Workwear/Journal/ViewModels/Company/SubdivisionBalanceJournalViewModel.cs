using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;

namespace workwear.Journal.ViewModels.Company
{
    public class SubdivisionBalanceJournalViewModel: JournalViewModelBase
    {
	    public Subdivision Subdivision {get;}
	    public DateTime OnDate { get; }
	    
        public SubdivisionBalanceJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigation,
            Subdivision subdivision,
            DateTime onDate) : base(unitOfWorkFactory, interactiveService, navigation)
        {
	        Subdivision = subdivision;
	        OnDate = onDate;
	        var dataLoader = new ThreadDataLoader<EmployeeBalanceJournalNode>(unitOfWorkFactory);
	        dataLoader.AddQuery(ItemsQuery);
	        DataLoader = dataLoader;
	        Title = $"Числится за подразделением - {Subdivision.Name}";
        }

        #region Query
        public IQueryOver<SubdivisionIssueOperation> ItemsQuery(IUnitOfWork unitOfWork)
        {
			SubdivisionBalanceJournalNode resultAlias = null;
			SubdivisionIssueOperation issueOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemTypesAlias = null;
			MeasurementUnits unitsAlias = null;
			SubdivisionIssueOperation removeIssueOperationAlias = null;
			WarehouseOperation warehouseOperationAlias = null;

			var expense = unitOfWork.Session.QueryOver(() => issueOperationAlias)
				.Where(e => e.Subdivision == Subdivision);

			var subQueryRemove = QueryOver.Of(() => removeIssueOperationAlias)
				.Where(() => removeIssueOperationAlias.IssuedOperation == issueOperationAlias)
				.Select (Projections.Sum<SubdivisionIssueOperation>(o => o.Returned));

			var balance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.Property(() => issueOperationAlias.Issued),
				Projections.SubQuery(subQueryRemove));

			return expense
				.JoinAlias(() => issueOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias)
				.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias)
				.JoinAlias(() => issueOperationAlias.WarehouseOperation, () => warehouseOperationAlias)
				.Where(e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
				.SelectList(list => list
					.SelectGroup(() => issueOperationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
					.Select(() => issueOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
					.Select(() => issueOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
					.Select(() => issueOperationAlias.ExpiryOn).WithAlias(() => resultAlias.ExpiryDate)
					.Select(balance).WithAlias(() => resultAlias.Balance)
				)
				.TransformUsing(Transformers.AliasToBean<SubdivisionBalanceJournalNode>());
        }
        #endregion
    }

    public class SubdivisionBalanceJournalNode
    {
	    public int Id { get; set; }
	    public string NomenclatureName { get; set;}
	    public string UnitsName { get; set;}
	    public decimal WearPercent { get; set;}
	    public DateTime IssuedDate { get; set;}
	    public DateTime? ExpiryDate { get; set;}
	    public double Percentage =>
		    ExpiryDate == null
			    ? 0
			    : (ExpiryDate.Value - DateTime.Today).TotalDays / (ExpiryDate.Value - IssuedDate).TotalDays;

	    public int Balance { get; set;}
	    public string BalanceText => $"{Balance} {UnitsName}";
    }
}