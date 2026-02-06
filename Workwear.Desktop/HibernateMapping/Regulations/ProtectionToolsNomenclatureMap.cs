using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace Workwear.HibernateMapping.Regulations {
	public class ProtectionToolsNomenclatureMap : ClassMap<ProtectionToolsNomenclature>{
		public ProtectionToolsNomenclatureMap() {
			Table("protection_tools_nomenclature");
			
			if(MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.CanChoose).Column("can_choose");
			References(x => x.ProtectionTools).Column("protection_tools_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
