using System;
using System.Linq;

namespace Workwear.Models.Import.Norms.DataTypes {
	public class DataTypeDepartment : DataTypeNormBase {
		public DataTypeDepartment()
		{
			ColumnNameKeywords.Add("отдел");
			ColumnNameKeywords.Add("отделы");
			ColumnNameKeywords.Add("участок");
			ColumnNameKeywords.Add("участки");
			ColumnNameKeywords.Add("группа");
			ColumnNameKeywords.Add("группы");
			Data = DataTypeNorm.Department;
		}
		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		private ChangeState GetChangeState(SheetRowNorm row, string value) {
			if(String.IsNullOrWhiteSpace(value)) {
				return new ChangeState(ChangeType.NotChanged);
			}
			
			return new ChangeState(row.NormItem.Norm.Posts.Any(p => p.Department?.Id == 0)
				? ChangeType.NewEntity
				: ChangeType.NotChanged);
		}
	}
}
