using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;

namespace Workwear.HibernateMapping.Stock {
	public class DutyNormIssueOperationMap : ClassMap<DutyNormIssueOperation> {
		public DutyNormIssueOperationMap() {
			Table("operation_issued_by_duty_norm");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.OperationTime).Column("operation_time").Not.Nullable();
			Map(x => x.WearPercent).Column("wear_percent").Not.Nullable();
			Map(x => x.Issued).Column("issued").Not.Nullable();
			Map(x => x.Returned).Column("returned").Not.Nullable();
			Map(x => x.UseAutoWriteoff).Column("auto_writeoff").Not.Nullable();
			Map(x => x.AutoWriteoffDate).Column("auto_writeoff_date");
			Map(x => x.StartOfUse).Column("start_of_use");
			Map(x => x.ExpiryByNorm).Column("expiry_by_norm");
			Map(x => x.Comment).Column("comment");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.DutyNorm).Column("duty_norm_id");
			References(x => x.DutyNormItem).Column("duty_norm_item_id");
			References(x => x.ProtectionTools).Column("protection_tools_id");
			References(x => x.WarehouseOperation).Column("warehouse_operation_id");
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
		}
	}
}
