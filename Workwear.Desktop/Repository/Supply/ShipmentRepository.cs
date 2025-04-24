using System.Collections.Generic;
using NHibernate;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;

namespace Workwear.Repository.Supply {
	public class ShipmentRepository {
		/// <summary>
		/// Возвращается первый склад из БД, если версия программы с единственным складом.
		/// Возвращает склад по умолчанию, определенный в настройках пользователя, при создании различных документов и прочее.
		/// </summary>
		public virtual IEnumerable<WarehouseOperation> GetIncomeWarehouseOperationsForShipment(IUnitOfWork uow, int shipmentId) {
			
			WarehouseOperation warehouseOperationAlias = null;
			Shipment shipmentAlias = null;
			Income incomeAlias = null;
			IncomeItem incomeItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			Owner ownerAlias = null;

			return uow.Session.QueryOver(() => warehouseOperationAlias)
				.JoinEntityAlias(() => incomeItemAlias, () => incomeItemAlias.WarehouseOperation.Id == warehouseOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeItemAlias.Document, () => incomeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => incomeAlias.Shipment, () => shipmentAlias, JoinType.LeftOuterJoin)
				.Where(() => shipmentAlias.Id == shipmentId)
				.List();
		}
/*		
		public virtual IEnumerable<WarehouseOperation> GetShipment(IUnitOfWork uow, int shipmentId) {
			uow.GetById<Expense>()
			

		}
		*/
	}
}
