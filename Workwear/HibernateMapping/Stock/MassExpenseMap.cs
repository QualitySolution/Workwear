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

			References(x => x.User).Column("user_id").Not.Nullable();
			References(x => x.Warehouseoperation).Column("warehouse_expense_id").Not.Nullable();

			 HasManyToMany(x => x.Employees).Table("stock_mass_expense_employee")
			.ParentKeyColumn("doc_id")
			.ChildKeyColumn("employee_id")
			.LazyLoad();


			HasMany(x => x.MassExpenseIssuing).KeyColumn("doc_id").Not.KeyNullable().Inverse().Cascade.AllDeleteOrphan()
			.LazyLoad();

			HasMany(x => x.MassExpenseOperation).KeyColumn("doc_id").Not.KeyNullable().Inverse().Cascade.AllDeleteOrphan()
			.LazyLoad();

		}
	}
}
