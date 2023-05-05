using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Import.Norms.DataTypes{
	public class DataTypeCondition : DataTypeNormBase {


		public DataTypeCondition( IList<NormCondition> conditions) {
			ColumnNameKeywords.Add("условия");
			ColumnNameKeywords.Add("условие");
			ColumnNameKeywords.Add("сезон");
			this.conditions = conditions;
			Data = DataTypeNorm.Condition;
		}

		private IList<NormCondition> conditions;

		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}

		private ChangeState GetChangeState(SheetRowNorm row, string value) {
			if(String.IsNullOrWhiteSpace(value)) {
				return new ChangeState(ChangeType.NotChanged);
			}

			var con = conditions.FirstOrDefault(c => c.Name == value);
			if(con != null) {
				row.AddSetValueAction(ValueSetOrder, () => row.NormItem.NormCondition = con);
				return new ChangeState(ChangeType.ChangeValue);
			}
			else 
				return new ChangeState(ChangeType.NotFound );
		}
	}
}
