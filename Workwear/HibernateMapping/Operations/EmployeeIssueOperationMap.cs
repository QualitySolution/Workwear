using FluentNHibernate.Mapping;
using workwear.Domain.Operations;

namespace workwear.HMap
{
	public class EmployeeIssueOperationMap : ClassMap<EmployeeIssueOperation>
	{
		public EmployeeIssueOperationMap()
		{
			Table ("operation_issued_by_employee");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.OperationTime).Column ("operation_time").Not.Nullable ();
			Map(x => x.Size).Column("size");
			Map(x => x.WearGrowth).Column("growth");
			Map(x => x.WearPercent).Column("wear_percent").Not.Nullable();
			Map(x => x.Issued).Column("issued").Not.Nullable();
			Map(x => x.Returned).Column("returned").Not.Nullable();
			Map(x => x.UseAutoWriteoff).Column("auto_writeoff").Not.Nullable();
			Map(x => x.AutoWriteoffDate).Column("auto_writeoff_date");
			Map(x => x.BuhDocument).Column("buh_document");
			Map(x => x.StartOfUse).Column("StartOfUse");
			Map(x => x.ExpiryByNorm).Column("ExpiryByNorm");

			References(x => x.Employee).Column("employee_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.NormItem).Column("norm_item_id");
			References(x => x.ProtectionTools).Column("protection_tools_id");
			References(x => x.IssuedOperation).Column("issued_operation_id");
			References(x => x.WarehouseOperation).Column("warehouse_operation_id");
		}
	}
}