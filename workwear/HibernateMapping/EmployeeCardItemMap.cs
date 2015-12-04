using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class EmployeeCardItemMap : ClassMap<EmployeeCardItem>
	{
		public EmployeeCardItemMap ()
		{
			Table ("wear_cards_item");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount");
			Map (x => x.Created).Column ("created");
			Map (x => x.LastIssue).Column ("last_issue");
			Map (x => x.NextIssue).Column ("next_issue");
			Map (x => x.RequiredByNorm).Column ("required_by_norm");

			References (x => x.ActiveNormItem).Column ("norm_item_id");
			References (x => x.Item).Column ("itemtype_id");
		}
	}
}