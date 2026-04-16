using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class SubdivisionMap : ClassMap<Subdivision>
	{
		public SubdivisionMap ()
		{
			Table ("subdivisions");

			if(MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.Code).Column("code");
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map (x => x.Address).Column ("address");
			Map (x => x.EmployeesColor).Column ("employees_color");
			Map(x => x.Comment).Column("comment");

			References(x => x.Warehouse).Column("warehouse_id");
			References(x => x.ParentSubdivision).Column("parent_subdivision_id");
			
			HasMany (x => x.ChildSubdivisions)
				.Inverse()
				.KeyColumn ("parent_subdivision_id").Not.KeyNullable()
				.LazyLoad();
		}
	}
}
