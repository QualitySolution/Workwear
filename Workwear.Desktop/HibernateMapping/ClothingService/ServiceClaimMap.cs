using FluentNHibernate.Mapping;
using Workwear.Domain.ClothingService;

namespace Workwear.HibernateMapping.ClothingService {
	public class ServiceClaimMap : ClassMap<ServiceClaim> {
		public ServiceClaimMap()
		{
			Table("clothing_service_claim");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.IsClosed).Column("is_closed");
			Map(x => x.NeedForRepair).Column("need_for_repair");
			Map(x => x.Defect).Column("defect");
			References(x => x.Barcode).Column("barcode_id");
			HasMany(x => x.States).Cascade.AllDeleteOrphan().Inverse().KeyColumn("claim_id");
		}
	}
}
