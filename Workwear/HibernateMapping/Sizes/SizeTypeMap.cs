using FluentNHibernate.Mapping;
using workwear.Domain.Sizes;

namespace workwear.HibernateMapping.Sizes
{
    public class SizeTypeMap: ClassMap<SizeType>
    {
        public SizeTypeMap()
        {
            Table("size_types");
            
            if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
                Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
            else 
                Id (x => x.Id).Column ("id").GeneratedBy.Native();

            Map(x => x.Name).Column("name");
            Map(x => x.UseInEmployee).Column("use_in_employee");
            Map(x => x.Category).Column("category");
            Map(x => x.Position).Column("position");
        }
    }
}