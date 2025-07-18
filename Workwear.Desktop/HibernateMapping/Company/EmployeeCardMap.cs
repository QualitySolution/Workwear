using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class EmployeeCardMap : ClassMap<EmployeeCard>
	{
		public EmployeeCardMap()
		{
			Table("employees");

			if (MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else {
				Id(x => x.Id).Column("id").GeneratedBy.Native();
				//Версионирование используем только в рабочей версии. В тестах вызывает ошибки.
				Version(x => x.LastUpdate).Column("last_update").Generated.Always();
			}

			Map(x => x.CardNumber).Column("card_number");
			Map(x => x.PersonnelNumber).Column("personnel_number");
			Map(x => x.CardKey).Column("card_key");
			Map(x => x.LastName).Column("last_name");
			Map(x => x.FirstName).Column("first_name");
			Map(x => x.Patronymic).Column("patronymic_name");
			Map(x => x.HireDate).Column("hire_date");
			Map(x => x.ChangeOfPositionDate).Column("change_of_position_date");
			Map(x => x.DismissDate).Column("dismiss_date");
			Map(x => x.BirthDate).Column("birth_date");
			Map(x => x.Email).Column("email");
			Map(x => x.Comment).Column("comment");

			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.LkRegistered).Column("lk_registered");

			Map(x => x.Photo).Column("photo").LazyLoad().CustomSqlType("BinaryBlob");
			Map(x => x.Sex).Column("sex").CustomType<SexStringType>();

			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.Department).Column("department_id");
			References(x => x.Post).Column("post_id");
			References(x => x.Leader).Column("leader_id");
			References(x => x.CreatedbyUser).Column("user_id");

			HasMany(x => x.WorkwearItems)
				.KeyColumn("employee_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan().Inverse()
				.LazyLoad();

			HasMany(x => x.Sizes).Table("employees_sizes")
				.KeyColumn("employee_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.BatchSize(100)
				.Inverse()
				.LazyLoad();

			HasManyToMany(x => x.UsedNorms).Table("employees_norms")
				.ParentKeyColumn("employee_id")
				.ChildKeyColumn("norm_id")
				.LazyLoad();

			HasManyToMany(x => x.DelatedDutyNorms).Table("duty_norms")
				.ParentKeyColumn("responsible_employee_id")
				.ChildKeyColumn("id")
				.LazyLoad();

			HasMany(x => x.Vacations).Table("employees_vacations")
				.KeyColumn("employee_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan().Inverse()
				.LazyLoad();
			
			HasMany(x => x.CostCenters).Table("employees_cost_allocation")
				.KeyColumn("employee_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();
			
			HasMany(x => x.EmployeeGroupItems).Table("employee_group_items")
				.KeyColumn("employee_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();
		}
	}
}
