using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class EmployeeCardMap : ClassMap<EmployeeCard>
	{
		public EmployeeCardMap ()
		{
			Table ("wear_cards");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.CardNumber).Column ("card_number");
			Map (x => x.LastName).Column ("last_name");
			Map (x => x.FirstName).Column ("first_name");
			Map (x => x.Patronymic).Column ("patronymic_name");
			Map (x => x.HireDate).Column ("hire_date");
			Map (x => x.DismissDate).Column ("dismiss_date");

			Map (x => x.Photo).Column ("photo").CustomSqlType ("BinaryBlob");
			Map (x => x.Sex).Column ("sex").CustomType<SexStringType> ();

			Map (x => x.WearGrowth).Column ("wear_growth");
			Map (x => x.WearSize).Column ("size_wear");
			Map (x => x.WearSizeStd).Column ("size_wear_std");
			Map (x => x.ShoesSize).Column ("size_shoes");
			Map (x => x.ShoesSizeStd).Column ("size_shoes_std");
			Map (x => x.HeaddressSize).Column ("size_headdress");
			Map (x => x.HeaddressSizeStd).Column ("size_headdress_std");
			Map (x => x.GlovesSize).Column ("size_gloves");
			Map (x => x.GlovesSizeStd).Column ("size_gloves_std");

			References (x => x.Facility).Column ("object_id");
			References (x => x.Post).Column ("post_id");
			References (x => x.Leader).Column ("leader_id");
			References (x => x.CreatedbyUser).Column ("user_id");
		}
	}
}