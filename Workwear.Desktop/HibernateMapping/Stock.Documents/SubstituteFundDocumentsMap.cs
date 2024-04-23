using FluentNHibernate.Mapping;
using QS.Project.Domain;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents 
{
	public class SubstituteFundDocumentsMap : ClassMap<SubstituteFundDocuments> 
	{
		public SubstituteFundDocumentsMap() 
		{
			Table("substitute_fund_documents");
			if (MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}

			Map(x => x.Date).Column("date").Not.Nullable();
			Map(x => x.CreationDate).Column("creation_date").Not.Nullable();
			Map(x => x.Comment).Column("comment").Nullable();
			References(x => x.CreatedbyUser).Column("user_id").Nullable();
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();
		}
	}
}
