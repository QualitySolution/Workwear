using FluentNHibernate.Mapping;
using Workwear.Domain.ClothingService;

namespace Workwear.HibernateMapping.ClothingService {
	public class ProvidedServiceMap  : ClassMap<ProvidedService> {
		public ProvidedServiceMap()
		{
			Table("clothing_service_services_claim");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Cost).Column("cost");

			References(x => x.Claim).Column("claim_id").Not.Nullable();
			References(x => x.Service).Column("service_id").Not.Nullable();
		}
	}
}
