using System.Collections.Generic;
using NHibernate;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;

namespace Workwear.Repository.Supply {
	public class ShipmentRepository {
		/// <summary>
		/// Складские операции считающиеся приходом по указаннуой закупке
		/// </summary>
		public virtual IEnumerable<WarehouseOperation> GetIncomeWarehouseOperationsForShipment(IUnitOfWork uow, int shipmentId) {
			
			WarehouseOperation warehouseOperationAlias = null;
			Shipment shipmentAlias = null;
			Income incomeAlias = null;
			IncomeItem incomeItemAlias = null;

			return uow.Session.QueryOver(() => warehouseOperationAlias)
				.JoinEntityAlias(() => incomeItemAlias, () => incomeItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeItemAlias.Document, () => incomeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeAlias.Shipment, () => shipmentAlias, JoinType.LeftOuterJoin)
				.Where(() => shipmentAlias.Id == shipmentId)
				.List();
		}
	}
}
