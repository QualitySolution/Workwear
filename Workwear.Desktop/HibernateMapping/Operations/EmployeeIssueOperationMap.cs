using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;

namespace Workwear.HibernateMapping.Stock
{
	public class EmployeeIssueOperationMap : ClassMap<EmployeeIssueOperation>
	{
		public EmployeeIssueOperationMap()
		{
			Table ("operation_issued_by_employee");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.OperationTime).Column ("operation_time").Not.Nullable ();
			Map(x => x.WearPercent).Column("wear_percent").Not.Nullable();
			Map(x => x.Issued).Column("issued").Not.Nullable();
			Map(x => x.Returned).Column("returned").Not.Nullable();
			Map(x => x.UseAutoWriteoff).Column("auto_writeoff").Not.Nullable();
			Map(x => x.AutoWriteoffDate).Column("auto_writeoff_date");
			Map(x => x.BuhDocument).Column("buh_document");
			Map(x => x.StartOfUse).Column("StartOfUse");
			Map(x => x.ExpiryByNorm).Column("ExpiryByNorm");
			Map(x => x.SignCardKey).Column("sign_key");
			Map(x => x.SignTimestamp).Column("sign_timestamp");
			Map(x => x.OverrideBefore).Column("override_before");
			Map(x => x.ManualOperation).Column("manual_operation");

			References(x => x.Employee).Column("employee_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.NormItem).Column("norm_item_id");
			References(x => x.ProtectionTools).Column("protection_tools_id");
			References(x => x.IssuedOperation).Column("issued_operation_id");
			References(x => x.WarehouseOperation).Column("warehouse_operation_id");
			References(x => x.EmployeeOperationIssueOnWriteOff).Column("operation_write_off_id");
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
		}
	}
}
