using System;
using System.Linq;
using System.Text.RegularExpressions;
using Workwear.Domain.Regulations;

namespace workwear.Models.Import.Norms.DataTypes {
	public class DataTypePost : DataTypeNormBase {
		public DataTypePost()
		{
			ColumnNameKeywords.Add("должность");
			ColumnNameKeywords.Add("должности");
			ColumnNameKeywords.Add("профессия");
			ColumnNameKeywords.Add("профессии");
			Data = DataTypeNorm.Post;
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
			
			var newPosts = row.SubdivisionPostCombination.Posts.Where(p => p.Id == 0).Select(x => x.Name).ToArray();
			var state = newPosts.Any() ? ChangeType.NewEntity : ChangeType.NotChanged;
			return new ChangeState(state, willCreatedValues: newPosts);
		}
		#endregion
	}
}
