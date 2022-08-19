using System;
using System.Text.RegularExpressions;

namespace workwear.Models.Import.Norms.DataTypes {
	public class DataTypeAmount : DataTypeNormBase {
		public DataTypeAmount()
		{
			ColumnNameKeywords.Add("норма выдачи");
			Data = DataTypeNorm.Amount;
		}

		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			var value = row.CellIntValue(target) ?? TryParseAmount(row.CellStringValue(target));
			row.AddColumnChange(target, GetChangeState(row, value, row.CellStringValue(target)));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowNorm row, int? value, string textValue) {
			if(value == null) {
				row.ProgramSkipped = true;
				return new ChangeState(ChangeType.ParseError);
			}
			
			if(row.NormItem.Amount == value.Value )
				return new ChangeState(ChangeType.NotChanged);

			var interpretedValue = value.Value.ToString() != textValue ? value.Value.ToString() : null;
			
			if(row.NormItem.Id == 0) {
				row.AddSetValueAction(ValueSetOrder,
					 () => row.NormItem.Amount = value.Value
				);
				return new ChangeState(ChangeType.NewEntity, interpretedValue: interpretedValue);
			}
			else {
				row.AddSetValueAction(ValueSetOrder,
					() => row.NormItem.Amount = value.Value
				);
				return new ChangeState(ChangeType.ChangeValue, oldValue: row.NormItem.Amount.ToString(), interpretedValue: interpretedValue);
			}
		}
		
		internal static int? TryParseAmount(string value) {
			if(String.IsNullOrWhiteSpace(value))
				return null;
			
			Regex regexp;
			Match match;
			
			regexp = new Regex(@"^(\d+) ?(пар|пара|пары|шт\.?|комплект.?)$");
			match = regexp.Match(value);
			if (match.Success)
			{
				return int.Parse(match.Groups[1].Value);
			}
			return null;
		}
		#endregion
	}
}
