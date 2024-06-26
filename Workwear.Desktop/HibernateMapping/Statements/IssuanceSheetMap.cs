﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Statements;

namespace Workwear.HibernateMapping.Statements
{
	public class IssuanceSheetMap : ClassMap<IssuanceSheet>
	{
		public IssuanceSheetMap()
		{
			Table("issuance_sheet");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.DocNumber).Column("doc_number");
			
			Map(x => x.Date).Column("date");

			References(x => x.Organization).Column("organization_id");
			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.Expense).Column("stock_expense_id");
			References(x => x.CollectiveExpense).Column("stock_collective_expense_id");
			References(x => x.ResponsiblePerson).Column("responsible_person_id");
			References(x => x.HeadOfDivisionPerson).Column("head_of_division_person_id");
			References(x => x.TransferAgent).Column("transfer_agent_id");

			HasMany(x => x.Items)
				.Inverse()
				.KeyColumn("issuance_sheet_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
		}
	}
}
