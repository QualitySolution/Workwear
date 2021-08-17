using System;
using NPOI.SS.UserModel;
using workwear.Domain.Company;
using workwear.Domain.Operations;

namespace workwear.Models.Import
{
	public class SheetRowWorkwearItems : SheetRowBase<DataTypeWorkwearItems>
	{
		public SheetRowWorkwearItems(IRow cells) : base(cells)
		{
		}

		public DateTime? Date;

		#region Найденные соответствия
		public EmployeeCard Employee;
		public EmployeeCardItem WorkwearItem;
		public EmployeeIssueOperation Operation;
		#endregion
	}
}
