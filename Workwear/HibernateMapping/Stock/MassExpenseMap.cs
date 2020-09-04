using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class MassExpenseMap : ClassMap<MassExpense>
	{
		public MassExpenseMap()
		{
			Table("stock_mass_expense");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Date).Column("date");
			Map(x => x.Comment).Column("comment");

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

			HasMany(x => x.MassExpenseOperation)
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
