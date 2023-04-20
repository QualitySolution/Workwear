using QS.DomainModel.Entity;
using Workwear.Domain.Company;

namespace Workwear.Domain.Stock.Documents {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "члены комиссии",
		Nominative = "член комиссии",
		Genitive = "члена комиссии"
	)]
	
	public class InspectionMember: IDomainObject {
		
		public virtual string Title =>
			$"{Member.Title} член комиссии в документе оценки №{Document.Id}";
		
		public virtual int Id { get; set; }
		public virtual Inspection Document { get; set; }
		public virtual Leader Member { get; set; }
	}
}
