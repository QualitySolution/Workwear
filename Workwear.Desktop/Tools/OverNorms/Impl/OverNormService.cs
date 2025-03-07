using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;

namespace Workwear.Tools.OverNorms.Impl 
{
	public class OverNormService : IOverNormService 
	{
		private readonly IUnitOfWork uow;

		public OverNormService(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}
		
		public IList<OverNormOperation> GetActualOverNormIssued(OverNormParam param, OverNormType? type = null, Warehouse warehouse = null) 
		{
			if (param == null) throw new ArgumentNullException(nameof(param));
			return OverNormOperationsQuery(param, type, warehouse).TransformUsing(Transformers.RootEntity).List();
		}

		private IQueryOver<OverNormOperation, OverNormOperation> OverNormOperationsQuery(OverNormParam param, OverNormType? type = null, Warehouse warehouse = null) 
		{
			OverNormOperation oonAlias = null;
			WarehouseOperation woAlias = null;

			return uow.Session.QueryOver(() => oonAlias)
				.JoinAlias(() => oonAlias.WarehouseOperation, () => woAlias)
				.Where(() => oonAlias.Employee == param.Employee)
				.Where(() => ((param.Nomenclature == null && param.Size == null && param.Height == null) ||
				              (woAlias.Nomenclature == param.Nomenclature && woAlias.WearSize == param.Size &&
				               woAlias.Height == param.Height)))
				.WithSubquery.WhereNotExists(QueryOver.Of<OverNormOperation>()
					.Where(oon => oon.WriteOffOverNormOperation.Id == oonAlias.Id)
					.Select(oon => oon.Id))
				.Where(() => type == null || oonAlias.Type == type)
				.Where(() => woAlias.ExpenseWarehouse != null && (warehouse == null || woAlias.ExpenseWarehouse == warehouse));
		}
	}
}
