using FluentNHibernate.Mapping;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.ClothingService {
	public class ServiceMap : ClassMap<Service> {
		public ServiceMap()
		{
			Table("clothing_service_services");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Cost).Column("cost");
			Map(x => x.Code).Column("code");
			Map(x => x.WithState).Column("with_state");
			Map(x => x.Comment).Column("comment");
			
			HasManyToMany<Nomenclature>(x => x.Nomenclatures)
				.Table("clothing_service_services_nomenclature")
				.ParentKeyColumn("service_id")
				.ChildKeyColumn("nomenclature_id");

		}
	}
}
