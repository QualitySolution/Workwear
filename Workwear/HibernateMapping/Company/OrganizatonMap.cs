using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class OrganizationMap : ClassMap<Organization>
	{
		public OrganizationMap()
		{
			Table ("organizations");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map(x => x.Name).Column("name");
			Map(x => x.Address).Column("address");
		}
	}
}