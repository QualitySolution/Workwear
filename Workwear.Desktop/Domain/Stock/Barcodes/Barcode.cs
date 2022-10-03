using QS.DomainModel.Entity;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Barcodes 
{
	public class Barcode : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; }

		private string value;
		public virtual string Value {
			get => value;
			set => SetField(ref this.value, value);
		}

		private string fractional;
		public virtual string Fractional {
			get => fractional;
			set => SetField(ref fractional, value);
		}

		private EmployeeIssueOperation employeeIssueOperation;
		public virtual EmployeeIssueOperation EmployeeIssueOperation {
			get => employeeIssueOperation;
			set => SetField(ref employeeIssueOperation, value);
		}
	}
}
