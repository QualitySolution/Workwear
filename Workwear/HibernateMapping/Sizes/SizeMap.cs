using FluentNHibernate.Mapping;
using Workwear.Domain.Sizes;

namespace workwear.HibernateMapping.Sizes
{
    public class SizeMap: ClassMap<Size>
    {
        public SizeMap()
        {
            Table("sizes");
            
            if(MappingParams.UseIdsForTest)
                Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
            else 
                Id (x => x.Id).Column ("id").GeneratedBy.Native();

            Map(x => x.Name).Column("name");
            Map(x => x.ShowInEmployee).Column("use_in_employee");
            Map(x => x.ShowInNomenclature).Column("use_in_nomenclature");
            Map(x => x.AlternativeName).Column("alternative_name");
            References(x => x.SizeType).Column("size_type_id");

            HasManyToMany(x => x.SuitableSizes)
                .Table("size_suitable")
                .ParentKeyColumn("size_id")
                .ChildKeyColumn("size_suitable_id");
        }
    }
}