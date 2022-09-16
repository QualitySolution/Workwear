using System;
using System.Linq;
using System.Text.RegularExpressions;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Import.Norms.DataTypes {
	public class DataTypeProtectionTools : DataTypeNormBase {
		public DataTypeProtectionTools()
		{
			ColumnNameKeywords.Add("номенклатура");
			ColumnNameKeywords.Add("одежды");
			ColumnNameKeywords.Add("обуви");
			ColumnNameKeywords.Add("средств");
			Data = DataTypeNorm.ProtectionTools;
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
			
			return new ChangeState(row.NormItem.ProtectionTools.Id == 0 ? ChangeType.NewEntity : ChangeType.NotChanged);
		}
		#endregion
	}
}
