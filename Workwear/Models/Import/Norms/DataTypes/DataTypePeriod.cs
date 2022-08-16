using System;
using System.Text.RegularExpressions;
using Workwear.Domain.Regulations;

namespace workwear.Models.Import.Norms.DataTypes {
	public class DataTypePeriod : DataTypeNormBase {
		public DataTypePeriod()
		{
			ColumnNameKeywords.Add("срок носки");
			ColumnNameKeywords.Add("сроки носки");
			Data = DataTypeNorm.Period;
		}

		public override void CalculateChange(SheetRowNorm row, ExcelValueTarget target) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowNorm row, string value) {
			if(String.IsNullOrWhiteSpace(value)) {
				row.ProgramSkipped = true;
				return new ChangeState(ChangeType.ParseError);
			}
			
			if(TryParsePeriod(value, out int periods, out NormPeriodType periodType, out string warning)) {
				if(row.NormItem.PeriodCount == periods && row.NormItem.NormPeriod == periodType)
					return new ChangeState(ChangeType.NotChanged);
				if(row.NormItem.Id == 0) {
					row.AddSetValueAction(ValueSetOrder,
						 () => MakeChange(row, periods, periodType)
					);
					return new ChangeState(ChangeType.NewEntity, warning: warning);
				}

				if(String.IsNullOrEmpty(warning)) {
					row.AddSetValueAction(ValueSetOrder,
						() => MakeChange(row, periods, periodType)
					);
					return new ChangeState(ChangeType.ChangeValue, oldValue: row.NormItem.Title);
				}
				else
					//Не меняем имеющееся значение при наличии предупреждения. Предполагаем что по поясам уже проставлено.
					return new ChangeState(ChangeType.NotChanged, warning: warning); 
			}
			else {
				row.ProgramSkipped = true;
				return new ChangeState(ChangeType.ParseError);
			}
		}

		void MakeChange(SheetRowNorm row, int periods, NormPeriodType periodType) {
			var item = row.NormItem;
			item.PeriodCount = periods;
			item.NormPeriod = periodType;
		}
		internal static bool TryParsePeriod(string value, out int periods, out NormPeriodType periodType, out string warning)
		{
			periods = 0;
			periodType = NormPeriodType.Wearout;
			warning = null;
			Regex regexp;
			Match match;
			
			if(value.ToLower().Contains("до износа")) {
				periodType = NormPeriodType.Wearout;
				return true;
			}
			if(value.ToLower().Contains("дежурн")) {
				periodType = NormPeriodType.Duty;
				return true;
			}
			//Количество в месяцев
			regexp = new Regex(@"(\d+) ?(месяц|месяца|месяцев|мес\.?)");
			match = regexp.Match(value);
			if(match.Success)
			{
				periodType = NormPeriodType.Month;
				periods = int.Parse(match.Groups[1].Value);
				return true;
			}

			//Указано количество лет, подразумевая выдачу одной единицы.
			regexp = new Regex(@"(\d+)([,\.]5)? *(год|года|лет)");
			match = regexp.Match(value);
			if (match.Success)
			{
				periodType = NormPeriodType.Year;
				periods = int.Parse(match.Groups[1].Value);
				if (match.Groups[2].Value.EndsWith(",5") || match.Groups[2].Value.EndsWith(".5")) {
					periods = periods * 12 + 6;
					periodType = NormPeriodType.Month;
				}

				return true;
			}
			return false;
		}

		#endregion
	}
}
