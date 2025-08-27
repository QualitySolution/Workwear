using FluentNHibernate.Mapping;
using Workwear.Domain.Visits;

namespace Workwear.HibernateMapping.Visits {
	public class IssuanceRequestMap: ClassMap<IssuanceRequest> {
		public IssuanceRequestMap() {
			Table("issuance_requests");
			
			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ReceiptDate).Column("receipt_date");
			Map(x => x.Status).Column("status");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References(x => x.CreatedByUser).Column("user_id");

			HasManyToMany(x => x.Employees)
				.Table("employees_issuance_request")
				.ParentKeyColumn("issuance_request_id")
				.ChildKeyColumn("employee_id")
				.LazyLoad();

			HasMany(x => x.CollectiveExpenses)
				.KeyColumn("issuance_request_id")
				.Inverse()
				.LazyLoad();
		}
	}
}
