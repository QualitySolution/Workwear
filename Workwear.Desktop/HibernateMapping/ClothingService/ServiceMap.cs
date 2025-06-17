using FluentNHibernate.Mapping;
using Workwear.Domain.ClothingService;

namespace Workwear.HibernateMapping.ClothingService {
	public class ServiceMap : ClassMap<Service> {
		public ServiceMap()
		{
			Table("clothing_service_services");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Cost).Column("cost");
			Map(x => x.Comment).Column("comment");

		}
	}
}
