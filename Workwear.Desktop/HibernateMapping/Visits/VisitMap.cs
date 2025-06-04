using FluentNHibernate.Mapping;
using Workwear.Domain.Visits;

namespace Workwear.HibernateMapping.Visits {
	public class VisitMap: ClassMap<Visit>
	{
		public VisitMap()
		{
			Table("visits");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.CreateDate).Column("create_date");
			Map(x => x.VisitDate).Column("visit_date");
			Map(x => x.EmployeeCreate).Column("employee_create");
			Map(x => x.Cancelled).Column("cancelled");
			Map(x => x.Done).Column("done");
			Map(x => x.Comment).Column("comment");
			
			References (x => x.Employee).Column ("employee_id");
			
			HasManyToMany(x => x.ExpenseDocuments)
				.Table("visits_documents")
				.ParentKeyColumn("visit_id")
				.ChildKeyColumn("expence_id");			
			HasManyToMany(x => x.WriteoffDocuments)
				.Table("visits_documents")
				.ParentKeyColumn("visit_id")
				.ChildKeyColumn("writeof_id");			
			HasManyToMany(x => x.ReturnDocuments)
				.Table("visits_documents")
				.ParentKeyColumn("visit_id")
				.ChildKeyColumn("return_id");
		}
	}
}
