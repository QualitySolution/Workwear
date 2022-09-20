using System;
using NPOI.SS.UserModel;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;

namespace Workwear.Models.Import.Issuance
{
	public class SheetRowWorkwearItems : SheetRowBase<SheetRowWorkwearItems>
	{
		public SheetRowWorkwearItems(IRow[] cells) : base(cells)
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
