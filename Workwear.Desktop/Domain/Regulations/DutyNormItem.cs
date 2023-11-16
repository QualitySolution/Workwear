using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Utilities;

namespace Workwear.Domain.Regulations {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "нормы выдачи",
		Nominative = "норма выдачи",
		PrepositionalPlural = "нормах выдачи",
		Genitive = "нормы выдачи"
	)]
	public class DutyNormItem : PropertyChangedBase, IDomainObject {
		#region Свойства
		public virtual int Id { get; set; }
		
		private DutyNorm dutyNorm;
		[Display (Name = "Норма")]
		public virtual DutyNorm DutyNorm {
			get { return dutyNorm; }
			set { SetField (ref dutyNorm, value, () => DutyNorm); }
		}

		private ProtectionTools protectionTools;
		[Display (Name = "Позиция")]
		public virtual ProtectionTools ProtectionTools {
			get { return protectionTools; }
			set { SetField (ref protectionTools, value); }
		}

		private int amount;
		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		private NormPeriodType normPeriod;
		[Display (Name = "Период нормы")]
		public virtual NormPeriodType NormPeriod {
			get { return normPeriod; }
			set { SetField (ref normPeriod, value, () => NormPeriod);
				if(value == NormPeriodType.Wearout || value == NormPeriodType.Duty)
					PeriodCount = 0;
			}
		}

		private int periodCount;
		[Display (Name = "Количество периодов")]
		public virtual int PeriodCount {
			get { return periodCount; }
			set { SetField (ref periodCount, value, () => PeriodCount); }
		}
		
		private string normParagraph;
		[Display(Name = "Пункт норм")]
		public virtual string NormParagraph {
			get => String.IsNullOrWhiteSpace(normParagraph) ? null : normParagraph; //Чтобы в базе хранить null, а не пустую строку.
			set => SetField(ref normParagraph, value); 
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		#endregion

		#region Вывод
		public virtual double AmountPerYear
		{
			get{
				double years = -1;
				switch(NormPeriod)
				{
					case NormPeriodType.Year:
						years = PeriodCount;
						break;
					case NormPeriodType.Month:
						years = (double)PeriodCount / 12;
						break;
					case NormPeriodType.Shift:
						years = (double)PeriodCount / 247;
						break;
					case NormPeriodType.Duty:
					case NormPeriodType.Wearout:
						return 0;
				}
				return Amount / years;
			}
		}

		public virtual int PeriodInMonths
		{
			get{
				switch(NormPeriod)
				{
					case NormPeriodType.Year:
						return PeriodCount * 12;
					case NormPeriodType.Month:
						return PeriodCount;
					case NormPeriodType.Shift:
						return PeriodCount / 21;
					case NormPeriodType.Duty:
					case NormPeriodType.Wearout:
						return 0;
				}
				return -1;
			}
		}

		public virtual string LifeText{
			get{
				switch(NormPeriod) {
					case NormPeriodType.Year:
						return NumberToTextRus.FormatCase (PeriodCount, "{0} год", "{0} года", "{0} лет");
					case NormPeriodType.Month:
						return NumberToTextRus.FormatCase (PeriodCount, "{0} месяц", "{0} месяца", "{0} месяцев");
					case NormPeriodType.Shift:
						return NumberToTextRus.FormatCase (PeriodCount, "{0} смена", "{0} смены", "{0} смен");
					case NormPeriodType.Wearout:
						return "До износа";
					default:
						return String.Empty;
				}
			}
		}
		#endregion
	}
}
