using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities;

namespace Workwear.Domain.Regulations
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки нормы",
		Nominative = "строка нормы",
		Genitive = "строки нормы"
		)]
	[HistoryTrace]
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
				if(value == NormPeriodType.Wearout || value == NormPeriodType.Duty)
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

		private string normParagraph;
		[Display(Name = "Пункт норм")]
		public virtual string NormParagraph {
			get => String.IsNullOrWhiteSpace(normParagraph) ? null : normParagraph; //Чтобы в базе хранить null, а не пустую строку.
			set => SetField(ref normParagraph, value); 
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => String.IsNullOrWhiteSpace(comment) ? null : comment; //Чтобы в базе хранить null, а не пустую строку. 
			set => SetField(ref comment, value); 
		}

		private bool isDisabled;
		[Display(Name = "Отключена строка нормы")]
		public virtual bool IsDisabled {
			get => isDisabled;
			set => SetField(ref isDisabled, value);
		}
		private DateTime lastUpdate;
		[Display(Name="Последнее обновление")]
		public virtual DateTime LastUpdate {
			get => lastUpdate; 
			set => SetField(ref lastUpdate, value); 
			
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
				case NormPeriodType.Duty:
				case NormPeriodType.Wearout:
					return 0;
				}
				return -1;
			}
		}

		public virtual string AmountText => ProtectionTools.Dispenser ?
			"Дозатор" :
			ProtectionTools?.Type?.Units?.MakeAmountShortStr(Amount);
		public virtual string LifeText{
			get {
				if(ProtectionTools.Dispenser)
					return String.Empty;
				switch(NormPeriod) {
					case NormPeriodType.Year:
					case NormPeriodType.Month:
						return PeriodCount + " " + PeriodText;
					case NormPeriodType.Wearout:
						return "До износа";
					case NormPeriodType.Duty:
						return "Дежурный";
					default:
						return String.Empty;
				}
			}
		}

		public virtual string PeriodText {
			get {
				switch(NormPeriod) {
					case NormPeriodType.Year:
						return NumberToTextRus.FormatCase(PeriodCount, "год", "года", "лет");
					case NormPeriodType.Month:
						return NumberToTextRus.FormatCase(PeriodCount, "месяц", "месяца", "месяцев");
					case NormPeriodType.Wearout:
						return "До износа";
					case NormPeriodType.Duty:
						return "Дежурный";
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
			if(NormPeriod == NormPeriodType.Wearout || NormPeriod == NormPeriodType.Duty)
				return null;
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
		public virtual DateTime? CalculateExpireDate(DateTime issueDate, decimal wearPercent = 0.0m)
		{
			if(NormPeriod == NormPeriodType.Wearout || NormPeriod == NormPeriodType.Duty)
				return null;
			var maxWriteofDate = issueDate.AddMonths(PeriodInMonths);
			return wearPercent == 0 ? maxWriteofDate : maxWriteofDate.AddDays((double)((maxWriteofDate - issueDate).Days * wearPercent * -1)) ;
		}

		public virtual string Title{
			get{ return String.Format ("{0} в количестве {1} на {2}", 
				ProtectionTools?.Name, AmountText, LifeText);
			}
		}

		public NormItem () { }

		/// <summary>
		/// Возвращает копию текущего объекта с привязкой к норме
		/// </summary>
		/// <returns>Копия текущего объекта NormItem.</returns>
		/// <param name="Norm">Норма, которой будет привязан возвращаемый объект NormItem</param>
		public virtual NormItem Copy(Norm norm)
		{
			NormItem newNormItem = new NormItem();
			newNormItem.norm = norm;
			newNormItem.protectionTools = this.ProtectionTools;
			newNormItem.normPeriod = this.normPeriod;
			newNormItem.periodCount = this.periodCount;
			newNormItem.amount = this.amount;
			newNormItem.normCondition = this.normCondition;
			newNormItem.normParagraph = this.normParagraph;
			newNormItem.comment = this.comment;
			return newNormItem;
		}
	}
}

