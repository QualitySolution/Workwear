using FluentNHibernate.Mapping;
using Workwear.Domain.Postomats;

namespace Workwear.HibernateMapping.Postomats 
{
	public class PostomatDocumentWithdrawMap : ClassMap<PostomatDocumentWithdraw> 
	{
		public PostomatDocumentWithdrawMap() 
		{
			Table("postomat_documents_withdraw");
			if (MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}

			Map(x => x.CreateTime).Column("create_time");
			Map(x => x.Comment).Column("comment");
			References (x => x.User).Column ("user_id").Nullable().Not.LazyLoad();
			
			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("document_withdraw_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
		}
	}
}
