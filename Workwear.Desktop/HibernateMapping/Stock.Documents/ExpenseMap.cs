﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class ExpenseMap : ClassMap<Expense>
	{
		public ExpenseMap ()
		{
			Table ("stock_expense");
			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.DocNumber).Column("doc_number");
			Map (x => x.Operation).Column ("operation");
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References (x => x.Subdivision).Column ("object_id");
			References (x => x.Employee).Column ("wear_card_id");
			References (x => x.CreatedbyUser).Column ("user_id");
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();

			HasOne(x => x.IssuanceSheet)
				.PropertyRef(x => x.Expense);

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_expense_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
