using QS.DomainModel.Entity;
using Workwear.Domain.Company;

namespace Workwear.Domain.Stock.Documents {
	public class InspectionMember: IDomainObject {
		public virtual int Id { get; set; }
		public virtual Inspection Document { get; set; }
		public virtual Leader Member { get; set; }
	}
}
