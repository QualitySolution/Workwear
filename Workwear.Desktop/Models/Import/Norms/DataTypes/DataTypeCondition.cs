using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Import.Norms.DataTypes{
	public class DataTypeCondition : DataTypeNormBase {


		public DataTypeCondition() {
			ColumnNameKeywords.Add("условия");
			ColumnNameKeywords.Add("условие");
			ColumnNameKeywords.Add("сезон");
			ColumnNameKeywords.Add("ограничения");
			ColumnNameKeywords.Add("ограничение");
			Data = DataTypeNorm.Condition;
		}

		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}

		private ChangeState GetChangeState(SheetRowNorm row, string value) {
			if(String.IsNullOrWhiteSpace(value)) {
				return new ChangeState(ChangeType.NotChanged);
			}

			if(row.NormItem.NormCondition.Id != 0) 
				return new ChangeState(ChangeType.ChangeValue);
			else
				return new ChangeState(ChangeType.NewEntity, willCreatedValues: new[] { row.NormItem.NormCondition.Name });
		}
	}
}
