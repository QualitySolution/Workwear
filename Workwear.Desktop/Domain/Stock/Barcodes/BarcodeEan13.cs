using QS.DomainModel.Entity;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Barcodes 
{
	public class BarcodeEan13 : IDomainObject
	{
		public virtual int Id { get; }
		public virtual string Value { get; set; }
		public virtual string Fractional { get; set; }
		public virtual EmployeeIssueOperation EmployeeIssueOperation { get; set; }
	}
}
