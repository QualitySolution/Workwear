using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Tools;

namespace Workwear.Domain.Regulations {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "нормы выдачи",
		Nominative = "норма выдачи",
		PrepositionalPlural = "нормах выдачи",
		Genitive = "нормы выдачи"
	)]
	public class DutyNormItem : PropertyChangedBase, IDomainObject {
		#region Хранимые Свойства
		public virtual int Id { get; set; }

		public virtual string Title => $@"{Amount} {ProtectionTools?.Type?.Units?.MakeAmountShortStr(Amount)}
			 ""{ProtectionTools.Name}"" на {PeriodCount} {PeriodText}";
		
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
		[PropertyChangedAlso(nameof(AmountUnitText))]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		private NormPeriodType normPeriod;
		[Display (Name = "Период нормы")]
		public virtual NormPeriodType NormPeriod {
			get { return normPeriod; }
			set { SetField (ref normPeriod, value, () => NormPeriod);
				if(value == NormPeriodType.Wearout)
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
		
		private DateTime? nextIssue;
		[Display (Name = "Дата следующей выдачи")]
		public virtual DateTime? NextIssue {
			get => nextIssue;
			set => SetField (ref nextIssue, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		#endregion

		#region Методы

		/// <summary>
		/// Рассчитывает дату износа пропорционально количеству выданного.
		/// </summary>
		public virtual DateTime? CalculateExpireDate(DateTime issueDate, int amount)
		{
			if(NormPeriod == NormPeriodType.Wearout || NormPeriod == NormPeriodType.Duty)
				return null;
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
		public virtual DateTime? CalculateExpireDate(DateTime issueDate, decimal wearPercent = 0.0m)
		{
			if(NormPeriod == NormPeriodType.Wearout || NormPeriod == NormPeriodType.Duty)
				return null;
			//TODO Некорректно считаем смены
			var maxWriteofDate = issueDate.AddMonths(PeriodInMonths);
			return wearPercent == 0 ? maxWriteofDate : maxWriteofDate.AddDays((double)((maxWriteofDate - issueDate).Days * wearPercent * -1)) ;
		}
		#endregion
		#region Граф

		public virtual IssueGraph<DutyNormIssueOperation> Graph { get; set; }
		
		public virtual int Issued(DateTime onDate) => Graph.AmountAtEndOfDay(onDate);
		
		/// <summary>
		/// Необходимое к выдачи количество.
		/// </summary>
		public virtual int CalculateRequiredIssue(BaseParameters parameters, DateTime onDate) {
			if(Graph == null)
				throw new NullReferenceException("Перед выполнением расчета CalculateRequiredIssue, Graph должен быть заполнен!");
			
			return Math.Max(0, Amount - Graph.UsedAmountAtEndOfDay(onDate.AddDays(parameters.ColDayAheadOfShedule)));
		}

		/// <summary>
		/// Обновляет дату следующей выдачи.
		/// </summary>
		public virtual void UpdateNextIssue(IUnitOfWork uow) {
			if(Graph == null) {
//Пока по простому
				if(Id == 0)
					Graph = new IssueGraph<DutyNormIssueOperation>();
				else {
					var query = uow.Session.QueryOver<DutyNormIssueOperation>()
						.Where(o => o.DutyNorm == DutyNorm && o.ProtectionTools == ProtectionTools);
					Graph = new IssueGraph<DutyNormIssueOperation>(query.List());
				}
			}

			DateTime? wantIssue = new DateTime();
			if(Graph.Intervals.Any()) {
				var listReverse = Graph.Intervals.OrderByDescending(x => x.StartDate).ToList();
				wantIssue = listReverse.First().StartDate;
				//Ищем первый с конца интервал где не хватает выданного до нормы.
				
				foreach(var interval in listReverse) {
					if(interval.CurrentCount < Amount)
						wantIssue = interval.StartDate;
					else
						break;
				}
			}
//Дата создания, если нужна. Пока в базе дата для строки не хранится	
			if(wantIssue == default(DateTime)) 
				wantIssue = DutyNorm.DateFrom ?? DateTime.Now;
			nextIssue = wantIssue;
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

		public virtual string PeriodText{
			get{
				switch(NormPeriod) {
					case NormPeriodType.Year:
						return NumberToTextRus.FormatCase (PeriodCount, "год", "года", "лет");
					case NormPeriodType.Month:
						return NumberToTextRus.FormatCase (PeriodCount, "месяц", "месяца", "месяцев");
					case NormPeriodType.Shift:
						return NumberToTextRus.FormatCase (PeriodCount, "сменy", "смены", "смен");
					case NormPeriodType.Wearout:
						return "До износа";
					default:
						return String.Empty;
				}
			}
		}

		public virtual string AmountUnitText(int a) {
			switch(ProtectionTools?.Type?.Units?.OKEI) {
				case "796":
					return NumberToTextRus.FormatCase(a, "штука", "штуки", "штук");
				case "715":
					return NumberToTextRus.FormatCase(a, "пара", "пары", "пар");
				case "839":
					return NumberToTextRus.FormatCase(a, "компл.", "компл.", "компл.");
				case "704":
					return NumberToTextRus.FormatCase(a, "набор", "набора", "наборов");
				default:
					return String.Empty;
			}
		}

		public virtual string AmountColor {
			get {
				var amount = Issued(DateTime.Today);
				if (Amount == amount)
					return "darkgreen";
				if (Amount < amount)
					return "blue";
				if (amount == 0)
					return "red";
				else
					return "orange";
			}
		}

		public virtual string NextIssueColor {
			get {
				if(DateTime.Today >= NextIssue)
					return "red";
				if(DateTime.Today > NextIssue)
					return "darkgreen";
				else
					return "black";
			}
		}

		#endregion
	}
}
