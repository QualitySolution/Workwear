using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
    public class EmployeeSizeMap : ClassMap<EmployeeSize>
    {
        public EmployeeSizeMap()
        {
            Table("wear_cards_sizes");
            
            if (MappingParams.UseIdsForTest)
                Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
            else
                Id(x => x.Id).Column("id").GeneratedBy.Native();

            References(x => x.Employee).Column("employee_id");
            References(x => x.Size).Column("size_id");
            References(x => x.SizeType).Column("size_type_id");
        }
    }
}