using System;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using workwear.Models.Company;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypeNameWithInitials : DataTypeEmployeeBase {
		public DataTypeNameWithInitials()
		{
			// ColumnNameKeywords.AddRange(new []{ });
			Data = DataTypeEmployee.NameWithInitials;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowEmployee row, string value) {
			if(String.IsNullOrWhiteSpace(value))
				return new ChangeState(ChangeType.NotChanged);
			
			var employee = row.EditingEmployee;

			value.SplitNameWithInitials(out var lastName, out var firstName, out var patronymic);
			var lastDiff = !String.IsNullOrEmpty(lastName) && !EmployeeParse.CompareString(employee.LastName, lastName);
			var firstDiff = !String.IsNullOrEmpty(firstName) && !EmployeeParse.CompareString(employee.FirstName?.FirstOrDefault().ToString(), firstName);
			var patronymicDiff = !String.IsNullOrEmpty(patronymic) && !EmployeeParse.CompareString(employee.Patronymic?.FirstOrDefault().ToString(), patronymic);
			string oldValue = (lastDiff || firstDiff || patronymicDiff) ? employee.FullName : null;
			if(!lastDiff && !firstDiff && !patronymicDiff)
				return new ChangeState(ChangeType.NotChanged);

			ChangeState state;
			if(row.EditingEmployee.Id == 0)
				state = new ChangeState(ChangeType.NewEntity);
			else
				state = new ChangeState(ChangeType.ChangeValue, oldValue: oldValue);
			
			if(lastDiff)
				row.AddSetValueAction(ValueSetOrder, () => employee.LastName = lastName);
			if(firstDiff) 
				row.AddSetValueAction(ValueSetOrder, () => employee.FirstName = firstName);
			if(patronymicDiff)
				row.AddSetValueAction(ValueSetOrder, () => employee.Patronymic = patronymic);

			return state;
		}
		#endregion
	}
}
