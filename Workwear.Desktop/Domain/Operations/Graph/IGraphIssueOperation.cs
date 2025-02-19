using System;
using QS.DomainModel.Entity;

namespace Workwear.Domain.Operations.Graph {
	public interface IGraphIssueOperation:IDomainObject {
		DateTime OperationTime { get; }
		DateTime? StartOfUse { get; }
		DateTime? AutoWriteoffDate { get; }
		int Issued { get; set; }
		int Returned { get; }
		bool OverrideBefore { get; }

		IGraphIssueOperation IssuedOperation { get; }
	}
}
