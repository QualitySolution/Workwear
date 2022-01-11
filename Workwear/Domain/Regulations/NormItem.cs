using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Utilities;
using Workwear.Domain.Regulations;

namespace workwear.Domain.Regulations
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки нормы",
		Nominative = "строка нормы")]
	public class NormItem : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		Norm norm;

		[Display (Name = "Норма")]
		public virtual Norm Norm {
			get { return norm; }
			set { SetField (ref norm, value, () => Norm); }
		}

		ProtectionTools protectionTools;

		[Display (Name = "Позиция")]
		public virtual ProtectionTools ProtectionTools {
			get { return protectionTools; }
			set { SetField (ref protectionTools, value); }
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
			set { SetField (ref normPeriod, value, () => NormPeriod);
				if(value == NormPeriodType.Wearout)
					PeriodCount = 0;
			  }
		}

		int periodCount;

		[Display (Name = "Количество периодов")]
		public virtual int PeriodCount {
			get { return periodCount; }
			set { SetField (ref periodCount, value, () => PeriodCount); }
		}

		private NormCondition normCondition;

		[Display(Name = "Условия нормы")]
		public virtual NormCondition NormCondition {
			get => normCondition; 
			set => SetField(ref normCondition, value); 
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

		/// <summary>
		/// Рассчитывает дату износа пропорционально количеству выданного.
		/// </summary>
		public virtual DateTime? CalculateExpireDate(DateTime issueDate, int amount)
		{
			if(NormPeriod == NormPeriodType.Wearout)
				return null;
			//TODO Некорректно считаем смены
			double oneItemByMonths = (double)PeriodInMonths / Amount;
			double months = amount * oneItemByMonths;
			int wholeMonths = (int)months;
			int addintionDays = (int)Math.Round((months - wholeMonths) * 30);
			if(Math.Abs((long)wholeMonths) > 100000) {
				logger.Warn("Расчет периода вышел за 100000 месяцев. Скорей всего изначальные данные некорректны.");
				return null;
			}
			return issueDate.AddMonths(wholeMonths).AddDays(addintionDays);
		}

		/// <summary>
		/// Рассчитывает дату износа по норме.
		/// </summary>
		public virtual DateTime? CalculateExpireDate(DateTime issueDate)
		{
			if(NormPeriod == NormPeriodType.Wearout)
				return null;
			//TODO Некорректно считаем смены
			return issueDate.AddMonths(PeriodInMonths);
		}

		public virtual string Title{
			get{ return String.Format ("{0} в количестве {1} на {2}", ProtectionTools?.Name, ProtectionTools?.Type?.Units?.MakeAmountShortStr (Amount), LifeText);
			}
		}

		public NormItem ()
		{
		}

		/// <summary>
		/// Возвращает копию текущего объекта без привязки к норме
		/// </summary>
		/// <returns>Копия текущего объекта NormItem.</returns>
		public virtual NormItem CopyNormItem()
		{
			NormItem newNormItem = new NormItem();
			newNormItem.protectionTools = this.ProtectionTools;
			newNormItem.normPeriod = this.normPeriod;
			newNormItem.periodCount = this.periodCount;
			newNormItem.amount = this.amount;
			return newNormItem;
		}

		/// <summary>
		/// Возвращает копию текущего объекта с привязкой к норме
		/// </summary>
		/// <returns>Копия текущего объекта NormItem.</returns>
		/// <param name="Norm">Норма, которой будет привязан возвращаемый объект NormItem</param>
		public virtual NormItem CopyNormItem(Norm norm)
		{
			NormItem newNormItem = new NormItem();
			newNormItem.norm = norm;
			newNormItem.protectionTools = this.ProtectionTools;
			newNormItem.normPeriod = this.normPeriod;
			newNormItem.periodCount = this.periodCount;
			newNormItem.amount = this.amount;
			return newNormItem;
		}
	}
}

