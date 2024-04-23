using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents 
{
	public class SubstituteFundDocumentItemMap : ClassMap<SubstituteFundDocumentItem> 
	{
		public SubstituteFundDocumentItemMap() 
		{
			Table("substitute_fund_document_items");
			if (MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}

			References(x => x.Document).Column("document_id").Not.Nullable();
			References(x => x.SubstituteFundOperation).Column("operation_subsitute_id").Not.Nullable();
		}
	}
}
