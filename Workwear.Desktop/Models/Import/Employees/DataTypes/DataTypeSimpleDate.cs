using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeSimpleDate : DataTypeEmployeeBase {
		private readonly PropertyInfo property;

		public DataTypeSimpleDate(DataTypeEmployee data, Expression<Func<EmployeeCard, DateTime?>> propertyName, string[] names) : base(data) {
			this.property = PropertyUtil.GetPropertyInfo(propertyName) ?? throw new ArgumentNullException(nameof(propertyName));
			ColumnNameKeywords.AddRange(names.Select(x => x.ToLower()));
		}
		

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellDateTimeValue(target);
			row.AddColumnChange(target, GetChangeState(row, value, row.CellStringValue(target)));
		}
		
		#region Helpers
		protected virtual ChangeState GetChangeState(SheetRowEmployee row, DateTime? value, string originalValue) {
			if(String.IsNullOrWhiteSpace(originalValue)) {
				return new ChangeState(ChangeType.NotChanged);
			}

			if(value == null)
				return new ChangeState(ChangeType.ParseError);
			
			var fieldValue = (DateTime?)property.GetValue(row.EditingEmployee);
			var interpretedValue = value.Value.ToShortDateString() != originalValue ? value.Value.ToShortDateString() : null; 
			if(fieldValue == value)
				return new ChangeState(ChangeType.NotChanged, interpretedValue: interpretedValue);
			
			row.AddSetValueAction(ValueSetOrder, () => property.SetValue(row.EditingEmployee, value));

			if(row.EditingEmployee.Id == 0)
				return new ChangeState(ChangeType.NewEntity, interpretedValue: interpretedValue);
			else
				return new ChangeState(ChangeType.ChangeValue, oldValue: fieldValue?.ToShortDateString(), interpretedValue: interpretedValue);
		}
		#endregion
	}
}
