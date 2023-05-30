using System;
using System.Linq;

namespace Workwear.Models.Import.Norms.DataTypes {
	public class DataTypeSubdivision : DataTypeNormBase {
		public DataTypeSubdivision()
		{
			ColumnNameKeywords.Add("подразделение");
			ColumnNameKeywords.Add("подразделения");
			Data = DataTypeNorm.Subdivision;
		}

		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowNorm row, string value) {
			if(String.IsNullOrWhiteSpace(value)) {
				return new ChangeState(ChangeType.NotChanged);
			}
			
			var newSubdivisions = row.SubdivisionPostCombination.Posts.Select(x => x.Subdivision).Distinct()
				.Where(x => x?.Id == 0).Select(x => x.Name).ToArray();
			return new ChangeState(newSubdivisions.Any() ? ChangeType.NewEntity : ChangeType.NotChanged, willCreatedValues: newSubdivisions);
		}
		#endregion
	}
}
