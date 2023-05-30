using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Import.Norms.DataTypes {
	public class DataTypeSimpleString : DataTypeNormBase {
		private readonly PropertyInfo property;
		private int maxLenght;

		public DataTypeSimpleString(DataTypeNorm data, Expression<Func<Norm, string>> propertyName, string[] names) : base(data) {
			this.property = PropertyUtil.GetPropertyInfo(propertyName) ?? throw new ArgumentNullException(nameof(propertyName));
			ColumnNameKeywords.AddRange(names.Select(x => x.ToLower()));
			
			var att = property.GetCustomAttributes(typeof(StringLengthAttribute), true);
			if(att.Length == 0)
				throw new InvalidOperationException($"Для свойства {property.Name} не задан необходимый атрибут {typeof(StringLengthAttribute)}.");
			maxLenght = ((StringLengthAttribute)att[0]).MaximumLength;
		}

		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		protected virtual ChangeState GetChangeState(SheetRowNorm row, string value) {
			if(String.IsNullOrWhiteSpace(value)) {
				return new ChangeState(ChangeType.NotChanged);
			}
			var fieldValue = (string)property.GetValue(row.NormItem.Norm);
			if(String.Equals(fieldValue, value, StringComparison.CurrentCultureIgnoreCase))
				return new ChangeState(ChangeType.NotChanged);

			if(value.Length > maxLenght)
				return new ChangeState(ChangeType.ParseError,
					error: $"Значение '{value}' слишком длинное для поля {property.GetTitle()}. Максимальная длинна {maxLenght}.");
			
			row.AddSetValueAction(ValueSetOrder, () => property.SetValue(row.NormItem.Norm, value));

			if(row.NormItem.Norm.Id == 0)
				return new ChangeState(ChangeType.NewEntity);
			else
				return new ChangeState(ChangeType.ChangeValue, oldValue: fieldValue);
		}
		#endregion
	}
}
