using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Company;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class MassExpenseEmployeeMap : ClassMap<MassExpenseEmployee>
	{
		public MassExpenseEmployeeMap()
		{

			Table("stock_mass_expense_employee");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Sex).Column("sex").CustomType<SexStringType>();

			Map(x => x.WearGrowth).Column("wear_growth");
			Map(x => x.WearSize).Column("size_wear");
			Map(x => x.WearSizeStd).Column("size_wear_std");
			Map(x => x.ShoesSize).Column("size_shoes");
			Map(x => x.ShoesSizeStd).Column("size_shoes_std");
			Map(x => x.WinterShoesSize).Column("size_winter_shoes");
			Map(x => x.WinterShoesSizeStd).Column("size_winter_shoes_std");
			Map(x => x.HeaddressSize).Column("size_headdress");
			Map(x => x.HeaddressSizeStd).Column("size_headdress_std");
			Map(x => x.GlovesSize).Column("size_gloves");
			Map(x => x.GlovesSizeStd).Column("size_gloves_std");

			References(x => x.EmployeeCard).Column("employee_id").Not.Nullable();
			References(x => x.DocumentMassExpense).Column("stock_mass_expense_id").Not.Nullable();
		}
	}
}
