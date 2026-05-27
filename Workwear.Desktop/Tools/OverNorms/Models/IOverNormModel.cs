using System.Collections.Generic;
using QS.Project.Domain;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.OverNorms.Impl;

namespace Workwear.Tools.OverNorms.Models 
{
	public interface IOverNormModel
	{
		bool Editable { get; }
	
		bool CanUseWithBarcodes { get; }
		
		bool CanUseWithoutBarcodes { get; }
		
		bool UseBarcodes { get; set; }
		
		bool CanChangeUseBarcodes { get; }
		
		bool RequiresEmployeeIssueOperation { get; }
		
		OverNorm CreateDocument(IList<OverNormParam> @params, Warehouse expenseWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null);

		void WriteOffOperation(OverNormOperation operation, Warehouse receiptWarehouse, UserBase createdByUser = null, string docNumber = null, string comment = null);

		void AddOperation(OverNorm document, OverNormParam param, Warehouse expenseWarehouse);
	}
}
