﻿using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees
{
	public class SheetRowEmployee : SheetRowBase<SheetRowEmployee>
	{
		public SheetRowEmployee(IRow[] cells) : base(cells)
		{
		}

		public List<EmployeeCard> Employees = new List<EmployeeCard>();

		public EmployeeCard EditingEmployee => Employees.FirstOrDefault();
	}
}
