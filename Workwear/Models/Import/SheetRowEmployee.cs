﻿using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using workwear.Domain.Company;

namespace workwear.Models.Import
{
	public class SheetRowEmployee : SheetRowBase<DataTypeEmployee>
	{
		public SheetRowEmployee(IRow cells) : base(cells)
		{
		}

		public List<EmployeeCard> Employees = new List<EmployeeCard>();

		public EmployeeCard EditingEmployee => Employees.FirstOrDefault();
	}
}
