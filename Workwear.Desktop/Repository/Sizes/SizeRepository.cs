using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Repository.Sizes
{
    public class SizeRepository {
        public IList<Size> GetSize(IUnitOfWork uow, bool fetchSuitableSizes = false) {
	        var query = uow.Session.QueryOver<Size>();
	        if(fetchSuitableSizes) {
		        uow.Session.QueryOver<Size>()
			        .Fetch(SelectMode.Fetch, s => s.SizesWhereIsThisSizeAsSuitable)
			        .Future();
		        uow.Session.QueryOver<Size>()
			        .Fetch(SelectMode.Fetch, s => s.SuitableSizes)
			        .Future();
	        }
	        return query.List();
        }

        public IList<SizeType> GetSizeType(IUnitOfWork uow) => 
            uow.Session.QueryOver<SizeType>()
                .OrderBy(x => x.Position).Asc
                .List();

        public IList<Size> GetUsedSizes(IUnitOfWork uow, Nomenclature[] nomenclatures) {
	        var ids = nomenclatures.Select(x => x.Id).ToArray();
	        var querySizeInWarehouse = QueryOver.Of<WarehouseOperation>()
		        .Where(x => x.Nomenclature.Id.IsIn(ids))
		        .Select(x => x.WearSize.Id);

	        var queryHeightInWarehouse = QueryOver.Of<WarehouseOperation>()
		        .Where(x => x.Nomenclature.Id.IsIn(ids))
		        .Select(x => x.Height.Id);
	        
	        var querySizeInEmployee = QueryOver.Of<EmployeeIssueOperation>()
		        .Where(x => x.Nomenclature.Id.IsIn(ids))
		        .Select(x => x.WearSize.Id);

	        var queryHeightInEmployee = QueryOver.Of<EmployeeIssueOperation>()
		        .Where(x => x.Nomenclature.Id.IsIn(ids))
		        .Select(x => x.Height.Id);

	        return uow.Session.QueryOver<Size>()
		        .Where(Restrictions.Disjunction()
			        .Add(Subqueries.WhereProperty<Size>(x => x.Id).In(querySizeInWarehouse))
			        .Add(Subqueries.WhereProperty<Size>(x => x.Id).In(queryHeightInWarehouse))
			        .Add(Subqueries.WhereProperty<Size>(x => x.Id).In(querySizeInEmployee))
			        .Add(Subqueries.WhereProperty<Size>(x => x.Id).In(queryHeightInEmployee))
		        )
		        .List();
        }
    }
}
