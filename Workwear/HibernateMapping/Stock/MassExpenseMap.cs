﻿using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class MassExpenseMap : ClassMap<MassExpense>
	{
		public MassExpenseMap()
		{
			Table("stock_mass_expense");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Date).Column("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References(x => x.CreatedbyUser).Column("user_id").Not.Nullable();
			References(x => x.WarehouseFrom).Column("warehouse_id").Not.Nullable();

			HasMany(x => x.ItemsNomenclature)
				.Inverse()
				.KeyColumn("stock_mass_expense_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();

			HasMany(x => x.ListEmployees)
				.Inverse()
				.KeyColumn("stock_mass_expense_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();

			HasMany(x => x.MassExpenseOperations)
				.Inverse()
				.KeyColumn("stock_mass_expense_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();

			HasOne(x => x.IssuanceSheet)
				.Cascade.All()
				.PropertyRef(x => x.MassExpense);

		}
	}
}