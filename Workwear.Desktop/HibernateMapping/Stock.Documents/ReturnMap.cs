﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class ReturnMap : ClassMap<Return>
	{
		public ReturnMap ()
		{
			Table ("stock_return");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.DocNumber).Column("doc_number");
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");
			
			References (x => x.EmployeeCard).Column ("employee_id");
			References (x => x.CreatedbyUser).Column ("user_id");
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_return_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
