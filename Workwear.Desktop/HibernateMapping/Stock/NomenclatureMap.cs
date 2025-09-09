﻿using FluentNHibernate.Mapping;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock
{
	public class NomenclatureMap : ClassMap<Nomenclature>
	{
		public NomenclatureMap ()
		{
			Table ("nomenclature");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name");
			Map(x => x.Sex).Column("sex");
			Map(x => x.Comment).Column("comment");
			Map(x => x.Number).Column("number");
			Map(x => x.AdditionalInfo).Column("additional_info");
			Map(x => x.Archival).Column("archival");
			Map(x => x.SaleCost).Column("sale_cost");
			Map(x => x.Rating).Column("rating");
			Map(x => x.RatingCount).Column("rating_count");
			Map(x => x.UseBarcode).Column("use_barcode");
			Map(x => x.Washable).Column("washable");

			References (x => x.Type).Column ("type_id");

			HasManyToMany<ProtectionTools>(x => x.ProtectionTools)
				.Table("protection_tools_nomenclature")
				.ParentKeyColumn("nomenclature_id")
				.ChildKeyColumn("protection_tools_id");
			
			HasManyToMany<Service>(x => x.UseServices)
				.Table("clothing_service_services_nomenclature")
				.ParentKeyColumn("nomenclature_id")
				.ChildKeyColumn("service_id");
		}
	}
}

