using System.Collections.Generic;
using NHibernate;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;

namespace Workwear.Repository.Supply {
	public class ShipmentRepository {
		
		private UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		
		public ShipmentRepository(UnitOfWorkProvider unitOfWorkProvider) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		
		/// <summary>
		/// Возвращает все строки заказанного, но еще не полученного товара по всем планируемым поставкам
		/// </summary>
		public virtual IList<ShipmentItem> GetOrderedItems() {
		    ShipmentItem shipmentItemAlias = null;
		    Shipment shipmentAlias = null;

		    var query = UoW.Session.QueryOver(() => shipmentItemAlias)
			    .JoinAlias(() => shipmentItemAlias.Shipment, () => shipmentAlias)
			    .Where(() => shipmentAlias.Status != ShipmentStatus.Draft) //Возможно нужно учитывать какие-то еще статусы. Пока не ясно какие.
			    .Where(() => shipmentItemAlias.Received < shipmentItemAlias.Ordered)
			    ;
		
		    return query.List();
		}
		
		/// <summary>
		/// Складские операции считающиеся приходом по указанной закупке
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
