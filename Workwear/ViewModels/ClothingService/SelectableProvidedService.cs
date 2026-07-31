using QS.ViewModels.Control;
using Workwear.Domain.ClothingService;

namespace Workwear.ViewModels.ClothingService {
	public class SelectableProvidedService : SelectableEntity<ProvidedService> {
		public SelectableProvidedService(int itemId, string name, int? entityId = null, ProvidedService entity = null)
			: base(itemId, name, entityId, entity) { }

		public virtual decimal Amount {
			get => Entity.Amount;
			set {
				if(Entity.Amount != value) {
					Entity.Amount = value;
					Entity.Cost = Entity.Service.Cost * value;
					OnPropertyChanged(nameof(Amount));
				}
			}
		}
	}
}
