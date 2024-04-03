using FluentNHibernate.Mapping;
using Workwear.Domain.Postomats;

namespace Workwear.HibernateMapping.Postomats 
{
	public class PostomatDocumentWithdrawItemMap : ClassMap<PostomatDocumentWithdrawItem> 
	{
		public PostomatDocumentWithdrawItemMap() 
		{
			Table ("postomat_document_withdraw_items");

			if(MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			References (x => x.DocumentWithdraw).Column ("document_withdraw_id").Not.Nullable ();
			References (x => x.Employee).Column("employee_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.Barcode).Column ("barcode_id");
			Map(x => x.TerminalId).Column("terminal_id");
			Map(x => x.TerminalLocation).Column("terminal_location");
		}
	}
}
