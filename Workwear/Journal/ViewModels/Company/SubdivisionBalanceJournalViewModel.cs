using System;
using System.Linq;
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
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
    public class SubdivisionBalanceJournalViewModel: JournalViewModelBase
    {
	    public SubdivisionBalanceFilterViewModel Filter { get; set; }

	    public SubdivisionBalanceJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigation,
            ILifetimeScope autofacScope,
            Subdivision subdivision = null) : base(unitOfWorkFactory, interactiveService, navigation)
        {
	        var dataLoader = new ThreadDataLoader<SubdivisionBalanceJournalNode>(unitOfWorkFactory);
	        dataLoader.AddQuery(ItemsQuery);
	        DataLoader = dataLoader;
	        JournalFilter = Filter = autofacScope.Resolve<SubdivisionBalanceFilterViewModel>(
		        new TypedParameter(typeof(JournalViewModelBase), this));
	        Filter.Subdivision = subdivision;
	        Title = subdivision != null 
		        ? $"Числится за подразделением - {Filter.Subdivision.Name}" 
		        : "Остатки по подразделениям";
	        //Журнал используется только для выбора. Если понадобится другой вариант, передавайте режим через конструктор.
	        SelectionMode = JournalSelectionMode.Multiple;
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
			Subdivision subdivisionAlias = null;

			var expense = unitOfWork.Session.QueryOver(() => issueOperationAlias);
	        
			if(Filter.Subdivision != null)
				expense.Where(e => e.Subdivision == Filter.Subdivision);

			var subQueryRemove = QueryOver.Of(() => removeIssueOperationAlias)
				.Where(() => removeIssueOperationAlias.IssuedOperation == issueOperationAlias)
				.Select (Projections.Sum<SubdivisionIssueOperation>(o => o.Returned));

			var balance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.Property(() => issueOperationAlias.Issued),
				Projections.SubQuery(subQueryRemove));
			if(Filter.Subdivision != null)
				expense
					.JoinAlias(() => issueOperationAlias.Nomenclature, () => nomenclatureAlias)
					.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => issueOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => issueOperationAlias.Subdivision, () => subdivisionAlias)
					.Where(e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > Filter.Date)
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.SelectList(list => list
						.SelectGroup(() => issueOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
						.Select(() => issueOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => issueOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => issueOperationAlias.ExpiryOn).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionName)
						.Select(balance).WithAlias(() => resultAlias.Balance));
			else {
				expense
					.JoinAlias(() => issueOperationAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureAlias.Type, () => itemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => itemTypesAlias.Units, () => unitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => issueOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => issueOperationAlias.Subdivision, () => subdivisionAlias)
					.Where(e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > Filter.Date)
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.SelectList(list => list
						.Select(() => issueOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => unitsAlias.Name).WithAlias(() => resultAlias.UnitsName)
						.Select(() => issueOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => issueOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => issueOperationAlias.ExpiryOn).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionName)
						.Select(balance).WithAlias(() => resultAlias.Balance));
				expense = expense.OrderBy(() => subdivisionAlias.Name).Asc;
			}
			return expense.TransformUsing(Transformers.AliasToBean<SubdivisionBalanceJournalNode>());
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
	    public string SubdivisionName { get; set; }
    }
}
