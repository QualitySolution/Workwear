namespace Workwear.Models.Import.Norms.DataTypes {
	public class DataTypeParagraph : DataTypeNormBase {
		public DataTypeParagraph() {
			ColumnNameKeywords.Add("пункт");
			ColumnNameKeywords.Add("документ");
			ColumnNameKeywords.Add("основание");
			ColumnNameKeywords.Add("обоснование");
			Data = DataTypeNorm.Paragraph;
		}

		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			row.AddColumnChange(target, GetChangeState(row, row.CellStringValue(target)));
		}

		private ChangeState GetChangeState(SheetRowNorm row, string value) {
			if( string.IsNullOrEmpty(value) || row.NormItem.NormParagraph == value)
				return new ChangeState(ChangeType.NotChanged);
			if(row.NormItem.Id == 0) {
				row.AddSetValueAction(ValueSetOrder,
					() => row.NormItem.NormParagraph = value
				);
				return new ChangeState(ChangeType.NewEntity, interpretedValue: value);
			} else {
				row.AddSetValueAction(ValueSetOrder,() => row.NormItem.NormParagraph = value);
				return new ChangeState(ChangeType.ChangeValue, oldValue: row.NormItem.Amount.ToString(),
					interpretedValue: value);
			}
		}
	}
}
