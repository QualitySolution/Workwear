using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class MassExpenseNomenclaturesMap : ClassMap<MassExpenseNomenclature>
	{
		public MassExpenseNomenclaturesMap()
		{
			Table("stock_mass_expense_nomenclatures");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Amount).Column("quantity");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.DocumentMassExpense).Column("stock_mass_expense_id").Not.Nullable();
		}
	}
}
