﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Communications;

namespace Workwear.HibernateMapping.Communications
{
	public class MessageTemplateMap: ClassMap<MessageTemplate>
	{
		public MessageTemplateMap()
		{
			Table("message_templates");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.MessageText).Column("message_text");
			Map(x => x.MessageTitle).Column("message_title");
			Map(x => x.LinkTitleText).Column("link_title");
			Map(x => x.LinkText).Column("link");
		}
	}
}
