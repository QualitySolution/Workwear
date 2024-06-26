﻿using System;
using System.Text.RegularExpressions;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Norms.DataTypes {
	public class DataTypePeriodAndCount : DataTypeNormBase {
		private readonly IImportNormSettings settings;

		public DataTypePeriodAndCount(IImportNormSettings settings)
		{
			this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
			ColumnNameKeywords.Add("количество и период");
			ColumnNameKeywords.Add("норма выдачи");
			Data = DataTypeNorm.PeriodAndCount;
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
			
			if(TryParsePeriodAndCount(value, out int amount, out int periods, out NormPeriodType periodType, out string warning)) {
				if(row.NormItem.Amount == amount && row.NormItem.PeriodCount == periods && row.NormItem.NormPeriod == periodType)
					return new ChangeState(ChangeType.NotChanged);
				if(row.NormItem.Id == 0) {
					row.AddSetValueAction(ValueSetOrder,
						 () => MakeChange(row, amount, periods, periodType)
					);
					return new ChangeState(ChangeType.NewEntity, warning: warning);
				}

				if(String.IsNullOrEmpty(warning)) {
					row.AddSetValueAction(ValueSetOrder,
						() => MakeChange(row, amount, periods, periodType)
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

		void MakeChange(SheetRowNorm row, int amount, int periods, NormPeriodType periodType) {
			var item = row.NormItem;						
			item.Amount = amount;
			item.PeriodCount = periods;
			item.NormPeriod = periodType;
		}
		internal bool TryParsePeriodAndCount(string value, out int amount, out int periods, out NormPeriodType periodType, out string warning)
		{
			amount = 0;
			periods = 0;
			periodType = NormPeriodType.Wearout;
			warning = null;
			Regex regexp;
			Match match;
			
			if(value.ToLower().Contains("до износа") || value.ToLower().Contains("до замены")) {
				regexp = new Regex(@"^(\d+) ?(пар|пара|пары|шт\.?|комплект.?)?");
				match = regexp.Match(value);
				if (match.Success)
					amount = int.Parse(match.Groups[1].Value);
				else 
					amount = 1;
				if(settings.WearoutToName) {
					periods = 1;
					periodType = NormPeriodType.Year;
				}
				else {
					periods = 0;
					periodType = NormPeriodType.Wearout;
				}
				return true;
			}
			if(value.ToLower().Contains("дежурн") || value.ToLower().Contains("деж.")) {
				amount = 1;
				periodType = NormPeriodType.Duty;
				return true;
			}
			//Количество в месяцев
			regexp = new Regex(@"(\d+).*(?:в|на)\s*(\d*)\s*(?:месяц|мес\.)");
			match = regexp.Match(value);
			if(match.Success)
			{
				periodType = NormPeriodType.Month;
				amount = int.Parse(match.Groups[1].Value);
				periods = String.IsNullOrEmpty(match.Groups[2].Value) ? 1 : int.Parse(match.Groups[2].Value);
				return true;
			}
			//Указано и количество выдачи и количество лет
			regexp = new Regex(@"(\d+).* (\d+)([,\.]5)? *(год|года|лет|г(\.| |\())");
			match = regexp.Match(value);
			if (match.Success)
			{
				periodType = NormPeriodType.Year;
				amount = int.Parse(match.Groups[1].Value);
				periods = int.Parse(match.Groups[2].Value);
				if (match.Groups[3].Value.EndsWith(",5") || match.Groups[3].Value.EndsWith(".5")) {
                	periods = periods * 12 + 6;
					periodType = NormPeriodType.Month;
				}

				return true;
			}
			//Указано только количество лет, подразумевая выдачу одной единицы.
			regexp = new Regex(@"(\d+)([,\.]5)? *(год|года|лет)");
			match = regexp.Match(value);
			if (match.Success)
			{
				periodType = NormPeriodType.Year;
				if(value.Contains("по поясам")) {
					amount = 0;
					warning = "Количество устанавливается по поясам. Необходимо в ручную проставить количество после импорта.";
				}
				else {
					amount = 1;
				}
				
				periods = int.Parse(match.Groups[1].Value);
				if (match.Groups[2].Value.EndsWith(",5") || match.Groups[2].Value.EndsWith(".5")) {
					periods = periods * 12 + 6;
					periodType = NormPeriodType.Month;
				}

				return true;
			}
			//Только количество подразумевая в 1 год.
			regexp = new Regex(@"^(\d+)([,\.](5|3+))?\s*(пар|пара|пары|шт\.?|комплект.?|кмп|компл\.|комп\.?)?( (в|на) год\.?)?$");
			match = regexp.Match(value);
			if (match.Success)
			{
				periods = 1;
				periodType = NormPeriodType.Year;
				if (match.Groups[2].Value.EndsWith("5")) {
					periods = 2;
					amount = 1;
				}
				else if (match.Groups[2].Value.EndsWith("3")) {
					periods = 3;
					amount = 1;
				} else {
					amount = int.Parse(match.Groups[1].Value);
				}
				return true;
			}
			return false;
		}

		#endregion
	}
}
