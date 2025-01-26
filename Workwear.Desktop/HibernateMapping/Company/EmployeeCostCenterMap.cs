using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company {
	public class EmployeeCostCenterMap : ClassMap<EmployeeCostCenter> {

		public EmployeeCostCenterMap() {
			Table("employees_cost_allocation");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Percent).Column("percent").Not.Nullable();
			
			References(x => x.Employee).Column("employee_id");
			References(x => x.CostCenter).Column("cost_center_id");
		}
	}
}
