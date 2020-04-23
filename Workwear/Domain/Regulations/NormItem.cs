using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Utilities;

namespace workwear.Domain.Regulations
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки нормы",
		Nominative = "строка нормы")]
	public class NormItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		Norm norm;

		[Display (Name = "Норма")]
		public virtual Norm Norm {
			get { return norm; }
			set { SetField (ref norm, value, () => Norm); }
		}

		ItemsType item;

		[Display (Name = "Позиция")]
		public virtual ItemsType Item {
			get { return item; }
			set { SetField (ref item, value, () => Item); }
		}

		int amount;

		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		NormPeriodType normPeriod;

		[Display (Name = "Период нормы")]
		public virtual NormPeriodType NormPeriod {
			get { return normPeriod; }
			set { SetField (ref normPeriod, value, () => NormPeriod); }
		}

		int periodCount;

		[Display (Name = "Количество периодов")]
		public virtual int PeriodCount {
			get { return periodCount; }
			set { SetField (ref periodCount, value, () => PeriodCount); }
		}

		#endregion

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
				}
				return -1;
			}
		}

		public virtual string LifeText{
			get{
				switch(NormPeriod)
				{
				case NormPeriodType.Year:
					return NumberToTextRus.FormatCase (PeriodCount, "{0} год", "{0} года", "{0} лет");
				case NormPeriodType.Month:
					return NumberToTextRus.FormatCase (PeriodCount, "{0} месяц", "{0} месяца", "{0} месяцев");
				case NormPeriodType.Shift:
					return NumberToTextRus.FormatCase (PeriodCount, "{0} смена", "{0} смены", "{0} смен");
				}
				return String.Empty;
			}
		}

		/// <summary>
		/// Рассчитывает дату износа пропорционально количеству выданного.
		/// </summary>
		public virtual DateTime CalculateExpireDate(DateTime issueDate, int amount)
		{
			//TODO Некорректно считаем смены
			double oneItemByMonths = (double)PeriodInMonths / Amount;
			double months = amount * oneItemByMonths;
			int wholeMonths = (int)months;
			int addintionDays = (int)Math.Round((months - wholeMonths) * 30);
			return issueDate.AddMonths(wholeMonths).AddDays(addintionDays);
		}

		/// <summary>
		/// Рассчитывает дату износа по норме.
		/// </summary>
		public virtual DateTime CalculateExpireDate(DateTime issueDate)
		{
			//TODO Некорректно считаем смены
			return issueDate.AddMonths(PeriodInMonths);
		}

		public virtual string Title{
			get{ return String.Format ("{0} в количестве {1} на {2}", Item?.Name, Item?.Units?.MakeAmountShortStr (Amount), LifeText);
			}
		}

		public NormItem ()
		{
		}
	}

	public enum NormPeriodType{
		[Display(Name = "Год")]
		Year,
		[Display(Name = "Месяц")]
		Month,
		[Display(Name = "Смена")]
		Shift,
	}

	public class NormPeriodTypeType : NHibernate.Type.EnumStringType
	{
		public NormPeriodTypeType () : base (typeof(NormPeriodType))
		{
		}
	}
}

