using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using workwear.Domain.Company;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypeSimpleString : DataTypeEmployeeBase {
		private readonly PropertyInfo property;

		public DataTypeSimpleString(DataTypeEmployee data, Expression<Func<EmployeeCard, string>> propertyName, string[] names) : base(data) {
			this.property = PropertyUtil.GetPropertyInfo(propertyName) ?? throw new ArgumentNullException(nameof(propertyName));
			ColumnNameKeywords.AddRange(names.Select(x => x.ToLower()));
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value, value));
		}
		
		#region Helpers
		protected virtual ChangeState GetChangeState(SheetRowEmployee row, string value, string originalValue) {
			if(String.IsNullOrWhiteSpace(value)) {
				return new ChangeState(ChangeType.NotChanged);
			}
			var fieldValue = (string)property.GetValue(row.EditingEmployee);
			var interpretedValue = value != originalValue ? value : null; 
			if(EmployeeParse.CompareString(fieldValue, value))
				return new ChangeState(ChangeType.NotChanged, interpretedValue: interpretedValue);
			
			row.AddSetValueAction(ValueSetOrder, () => property.SetValue(row.EditingEmployee, value));

			if(row.EditingEmployee.Id == 0)
				return new ChangeState(ChangeType.NewEntity, interpretedValue: interpretedValue);
			else
				return new ChangeState(ChangeType.ChangeValue, oldValue: fieldValue, interpretedValue: interpretedValue);
		}
		#endregion
	}
}
