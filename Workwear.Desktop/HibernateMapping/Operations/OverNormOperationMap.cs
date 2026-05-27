using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock 
{
	public class OverNormOperationMap : ClassMap<OverNormOperation> 
	{
		public OverNormOperationMap() 
		{
			Table("operation_over_norm");
			if (MappingParams.UseIdsForTest) 
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else 
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.OperationTime).Column("operation_time").Not.Nullable();
			Map(x => x.LastUpdate).Column("last_update").Not.Nullable();
			Map(x => x.Type).Column("type").Not.Nullable();

			References(x => x.Employee).Column("employee_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
			References(x => x.WarehouseOperation).Column("operation_warehouse_id").Not.Nullable().Cascade.All();
			References(x => x.SubstitutedIssueOperation).Column("substituted_issue_operation_id").Nullable();
            References(x => x.ReturnFromOperation).Column("return_from_operation").Nullable().Cascade.All();
            
			HasMany(x => x.BarcodeOperations)
				.KeyColumn("over_norm_operation_id").Inverse()
				.Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
			
			HasManyToMany<Barcode>(x => x.Barcodes)
				.Table("operation_barcodes")
				.ParentKeyColumn("over_norm_operation_id")
				.ChildKeyColumn("barcode_id");
		}
	}
}
