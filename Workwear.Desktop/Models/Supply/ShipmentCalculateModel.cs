using System;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Supply;
using Workwear.Repository.Supply;

namespace Workwear.Models.Supply {
	public class ShipmentCalculateModel {
		
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private readonly ShipmentRepository shipmentRepository;
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		
		public ShipmentCalculateModel(UnitOfWorkProvider unitOfWorkProvider,ShipmentRepository shipmentRepository) {
			this.unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
			this.shipmentRepository = shipmentRepository ?? throw new ArgumentNullException(nameof(shipmentRepository));
		}
		
		public void UpdateShipment(int shipment_id) {
			var doc = UoW.GetById<Shipment>(shipment_id);
			if(doc == null)
				return;
			var operations = shipmentRepository.GetIncomeWarehouseOperationsForShipment(UoW, shipment_id);
			var opDictionary = operations.GroupBy(o => (o.Nomenclature.Id, o.WearSize?.Id ?? -1, o.Height?.Id ?? -1))
				.ToDictionary(g => g.Key, r => r.Sum(o => o.Amount));
			
			foreach(var item in doc.Items) {
				int r;
				opDictionary.TryGetValue((item.Nomenclature.Id, item.WearSize?.Id ?? -1, item.Height?.Id ?? -1), out r);
				item.Received = r;
				UoW.Save(item);
			}
			
			doc.FullReceived = doc.Items.All(i => i.Received >= i.Ordered);
			doc.HasReceive = operations.Any();
			UoW.Save(doc);
			UoW.Commit();
		}
	}
}
