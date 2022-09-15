using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;

namespace workwear.HMap
{
	public class SubdivisionIssueOperationMap : ClassMap<SubdivisionIssueOperation>
	{
		public SubdivisionIssueOperationMap()
		{
			Table ("operation_issued_in_subdivision");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.OperationTime).Column ("operation_time").Not.Nullable ();
			Map(x => x.WearPercent).Column("wear_percent").Not.Nullable();
			Map(x => x.Issued).Column("issued").Not.Nullable();
			Map(x => x.Returned).Column("returned").Not.Nullable();
			Map(x => x.UseAutoWriteoff).Column("auto_writeoff").Not.Nullable();
			Map(x => x.AutoWriteoffDate).Column("auto_writeoff_date");
			Map(x => x.StartOfUse).Column("start_of_use");
			Map(x => x.ExpiryOn).Column("expiry_on");

			References(x => x.Subdivision).Column("subdivision_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.SubdivisionPlace).Column("subdivision_place_id");
			References(x => x.IssuedOperation).Column("issued_operation_id");
			References(x => x.WarehouseOperation).Column("warehouse_operation_id");
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
		}
	}
}
