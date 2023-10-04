using FluentNHibernate.Mapping;
using Workwear.Domain.ClothingService;

namespace Workwear.HibernateMapping.ClothingService {
	public class StateOperationMap : ClassMap<StateOperation> {
		public StateOperationMap()
		{
			Table("clothing_service_states");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.State).Column("state");
			References(x => x.Claim).Column("claim_id");
		}
	}
}
