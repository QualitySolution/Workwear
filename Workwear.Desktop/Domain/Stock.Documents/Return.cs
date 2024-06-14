using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Stock.Documents {
	public class Return: StockDocument, IValidatableObject {
		
		
		
		
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			throw new System.NotImplementedException();
		}
	}
}
