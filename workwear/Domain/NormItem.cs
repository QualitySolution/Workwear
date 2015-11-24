using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace workwear.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки нормы",
		Nominative = "строка нормы")]
	public class NormItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

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

