using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company {
	public class EmployeeSelectedNomenclatureMap : ClassMap<EmployeeSelectedNomenclature> {
		public EmployeeSelectedNomenclatureMap() {
			Table("employees_selected_nomenclatures");

			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Employee).Column("employee_id");
			References(x => x.ProtectionTools).Column("protection_tools_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
