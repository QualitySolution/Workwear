﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents {
	public class BarcodingMap : ClassMap<Barcoding>{
		public BarcodingMap() {
			Table("stock_barcoding");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.DocNumber).Column("doc_number");
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");
			References (x => x.CreatedbyUser).Column ("user_id");
			References (x => x.Warehouse).Column ("warehouse_id");

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_barcoding_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}	
}
