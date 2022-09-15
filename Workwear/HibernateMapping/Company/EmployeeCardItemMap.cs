using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class EmployeeCardItemMap : ClassMap<EmployeeCardItem>
	{
		public EmployeeCardItemMap ()
		{
			Table ("wear_cards_item");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Amount).Column ("amount");
			Map (x => x.Created).Column ("created");
			Map (x => x.LastIssue).Column ("last_issue");
			Map (x => x.NextIssue).Column ("next_issue");
			Map (x => x.NextIssueAnnotation).Column("next_issue_annotation");

			References (x => x.EmployeeCard).Column ("wear_card_id").Not.Nullable ();
			References (x => x.ActiveNormItem).Column ("norm_item_id"); //.Not.Nullable (); Из за странной работы NHibernate при удалении по зависимости он это свойство переключает в Null и падает с эксепшен, даже если в той же транзакции эта строка будет удалена.
			References (x => x.ProtectionTools).Column ("protection_tools_id").Not.Nullable ();
		}
	}
}
