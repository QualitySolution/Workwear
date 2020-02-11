using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class MassExpenseIssuingMap : ClassMap<MassExpenseIssuing>
	{
		public MassExpenseIssuingMap()
		{
			Table("stock_mass_expense_issuing");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Amount).Column("quantity");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.MassExpense).Column("doc_id").Not.Nullable();

		}
	}
}
