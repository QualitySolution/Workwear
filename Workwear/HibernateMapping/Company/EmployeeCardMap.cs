using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class EmployeeCardMap : ClassMap<EmployeeCard>
	{
		public EmployeeCardMap ()
		{
			Table ("wear_cards");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.CardNumber).Column ("card_number");
			Map (x => x.PersonnelNumber).Column ("personnel_number");
			Map (x => x.LastName).Column ("last_name");
			Map (x => x.FirstName).Column ("first_name");
			Map (x => x.Patronymic).Column ("patronymic_name");
			Map (x => x.HireDate).Column ("hire_date");
			Map(x => x.ChangeOfPositionDate).Column("change_of_position_date");
			Map (x => x.DismissDate).Column ("dismiss_date");
			Map(x => x.Comment).Column("comment");

			#region NLMK
			Map(x => x.ProfessionId).Column("nlmk_prof_id");
			Map(x => x.SubdivisionId).Column("nlmk_subdivision_id");
			Map(x => x.DepartmentId).Column("nlmk_dept_id");
			Map(x => x.PostId).Column("nlmk_post_id");
			#endregion

			Map(x => x.Photo).Column("photo").LazyLoad().CustomSqlType ("BinaryBlob");
			Map (x => x.Sex).Column ("sex").CustomType<SexStringType> ();

			Map (x => x.WearGrowth).Column ("wear_growth");
			Map (x => x.WearSize).Column ("size_wear");
			Map (x => x.WearSizeStd).Column ("size_wear_std");
			Map (x => x.ShoesSize).Column ("size_shoes");
			Map (x => x.ShoesSizeStd).Column ("size_shoes_std");
			Map(x => x.WinterShoesSize).Column("size_winter_shoes");
			Map(x => x.WinterShoesSizeStd).Column("size_winter_shoes_std");
			Map (x => x.HeaddressSize).Column ("size_headdress");
			Map (x => x.HeaddressSizeStd).Column ("size_headdress_std");
			Map (x => x.GlovesSize).Column ("size_gloves");
			Map (x => x.GlovesSizeStd).Column ("size_gloves_std");

			//References (x => x.Subdivision).Column ("object_id");
			//References(x => x.Department).Column("department_id");
			//References (x => x.Post).Column ("post_id");
			References (x => x.Leader).Column ("leader_id");
			References (x => x.CreatedbyUser).Column ("user_id");

			HasMany (x => x.WorkwearItems)
				.KeyColumn ("wear_card_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ().Inverse ()
				.LazyLoad ();

			HasManyToMany (x => x.UsedNorms).Table ("wear_cards_norms")
				.ParentKeyColumn ("wear_card_id")
				.ChildKeyColumn ("norm_id")
				.LazyLoad ();

			HasMany(x => x.Vacations).Table("wear_cards_vacations")
				.KeyColumn("wear_card_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan().Inverse()
				.LazyLoad();
		}
	}
}