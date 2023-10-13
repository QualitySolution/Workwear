using FluentNHibernate.Mapping;
using Workwear.Domain.Postomats;

namespace Workwear.HibernateMapping.Postomats
{
	public class PostomatDocumentMap : ClassMap<PostomatDocument>
	{
		public PostomatDocumentMap ()
		{
			Table ("postomat_documents");
			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.TerminalId).Column("terminal_id");
			Map(x => x.CreateTime).Column("create_time");
			Map (x => x.Status).Column ("status");
			Map (x => x.Type).Column ("type");
			Map(x => x.Comment).Column("comment");

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("document_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
