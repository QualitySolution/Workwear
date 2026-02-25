using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.ClothingService {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "оказанные услуги",
		Nominative = "оказанная услуга",
		Genitive = "оказанной услуги",
		GenitivePlural = "оказанных услуг"
	)]
	[HistoryTrace]
	public class ProvidedService  : PropertyChangedBase, IDomainObject {
		public virtual int Id { get; set; }
		
		public ProvidedService() {}
		public ProvidedService(ServiceClaim claim, Service service) {
			this.Claim = claim;
			this.Service = service;
			this.Cost = service.Cost;
		}
		
		private ServiceClaim claim;
		[Display(Name = "Заявка")]
		public virtual ServiceClaim Claim {
			get => claim;
			set => SetField(ref claim, value);
		}
		
		private Service service;
		[Display(Name = "Услуга")]
		public virtual Service Service {
			get => service;
			set => SetField(ref service, value);
		}
		
		private decimal cost;
		[Display(Name = "Стоимость")]
		public virtual decimal Cost {
			get => cost;
			set => SetField(ref cost, value);
		}
	}
}
