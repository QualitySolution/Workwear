using FluentNHibernate.Mapping;
using workwear.Domain.Users;

namespace workwear.HibernateMapping.User
{
	public class UserSettingsMap : ClassMap<UserSettings>
	{
		public UserSettingsMap ()
		{
			Table ("user_settings");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.ToolbarStyle).Column ("toolbar_style").CustomType<ToolbarStyleStringType>();
			Map (x => x.ToolBarIconsSize).Column ("toolbar_icons_size").CustomType<ToolBarIconsSizeStringType>();
			Map(x => x.ShowToolbar).Column("toolbar_show");

			References(x => x.DefaultWarehouse).Column("default_warehouse_id").Cascade.All();
			References(x => x.DefaultLeader).Column("default_leader_id").Cascade.All();
			References(x => x.DefaultOrganization).Column("default_organization_id").Cascade.All();
			References(x => x.DefaultResponsiblePerson).Column("default_responsible_person_id").Cascade.All(); 

			References (x => x.User).Column ("user_id");
		}
	}
}