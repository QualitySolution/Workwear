using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
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
		/// Возвращает общее количество заказанного, но еще не полученного товара по всем планируемым поставкам
		/// с дополнительной информацией о размере, росте и количестве заказанного
		/// </summary>
		public virtual IList<OrderedInfo> GetOrderedAmount() {
		    ShipmentItem shipmentItemAlias = null;
		    Nomenclature nomenclatureAlias = null;
		    Shipment shipmentAlias = null;
		    OrderedInfo orderedInfoAlias = null;
			//var sumProjection = Projections.Sum();

		    var query = UoW.Session.QueryOver(() => shipmentItemAlias)
			    .JoinAlias(() => shipmentItemAlias.Nomenclature, () => nomenclatureAlias)
			    .JoinAlias(() => shipmentItemAlias.Shipment, () => shipmentAlias)
			    .Where(() => shipmentAlias.Status != ShipmentStatus.Draft) //Возможно нужно учитывать какие-то еще статусы. Пока не ясно какие.
			    .Where(() => shipmentItemAlias.Received < shipmentItemAlias.Ordered)
			    .SelectList(list => list
				    .SelectGroup(() => shipmentItemAlias.Nomenclature.Id).WithAlias(() => orderedInfoAlias.NomenclatureId)
				    .SelectGroup(() => shipmentItemAlias.WearSize).WithAlias(() => orderedInfoAlias.WearSize)
				    .SelectGroup(() => shipmentItemAlias.Height).WithAlias(() => orderedInfoAlias.Height)
				    .SelectSum(() => shipmentItemAlias.Ordered - shipmentItemAlias.Received).WithAlias(() => orderedInfoAlias.Amount)
			    );
		
		    return query.TransformUsing(Transformers.AliasToBean<OrderedInfo>())
			    .List<OrderedInfo>()
			    .ToList();
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
