using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeSimpleString : DataTypeEmployeeBase {
		private readonly PropertyInfo property;
		private int maxLenght;

		public DataTypeSimpleString(DataTypeEmployee data, Expression<Func<EmployeeCard, string>> propertyName, string[] names) : base(data) {
			this.property = PropertyUtil.GetPropertyInfo(propertyName) ?? throw new ArgumentNullException(nameof(propertyName));
			ColumnNameKeywords.AddRange(names.Select(x => x.ToLower()));
			
			var att = property.GetCustomAttributes(typeof(StringLengthAttribute), true);
			if(att.Length == 0)
				throw new InvalidOperationException($"Для свойства {property.Name} не задан необходимый атрибут {typeof(StringLengthAttribute)}.");
			maxLenght = ((StringLengthAttribute)att[0]).MaximumLength;
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

			if(value.Length > maxLenght)
				return new ChangeState(ChangeType.ParseError,
					error: $"Значение '{value}' слишком длинное для поля {property.GetTitle()}. Максимальная длинна {maxLenght}.");
			
			row.AddSetValueAction(ValueSetOrder, () => property.SetValue(row.EditingEmployee, value));

			if(row.EditingEmployee.Id == 0)
				return new ChangeState(ChangeType.NewEntity, interpretedValue: interpretedValue);
			else
				return new ChangeState(ChangeType.ChangeValue, oldValue: fieldValue, interpretedValue: interpretedValue);
		}
		#endregion
	}
}
