using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Models.Operations;
using Workwear.Tools;

namespace Workwear.Domain.Regulations {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки дежурной нормы",
		Nominative = "строка дежурной нормы",
		PrepositionalPlural = "строках дежурной нормы",
		Genitive = "строки дежурной нормы"
	)]
	public class DutyNormItem : PropertyChangedBase, IDomainObject {
		
		#region Хранимые Свойства
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
		[PropertyChangedAlso(nameof(AmountUnitText))]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		private DutyNormPeriodType normPeriod;
		[Display (Name = "Период нормы")]
		public virtual DutyNormPeriodType NormPeriod {
			get { return normPeriod; }
			set { if( SetField (ref normPeriod, value, () => NormPeriod)) 
					if(value == DutyNormPeriodType.Wearout)
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
			if(NormPeriod == DutyNormPeriodType.Wearout)
				return null;
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
			if(NormPeriod == DutyNormPeriodType.Wearout)
				return null;
			var maxWriteofDate = issueDate.AddMonths(PeriodInMonths);
			return wearPercent == 0 ? maxWriteofDate : maxWriteofDate.AddDays((double)((maxWriteofDate - issueDate).Days * wearPercent * -1)) ;
		}
		#endregion

		#region Работа со складом
		public virtual StockBalanceModel StockBalanceModel { get; set; }
		
		/// <summary>
		/// Получаем значения остатков на складе для подходящих позиций.
		/// ВНИМАНИЕ! StockBalanceModel должна быть заполнена!
		/// </summary>
		public virtual IEnumerable<StockBalance> InStock {
			get { 				
				if(StockBalanceModel == null)
					throw new InvalidOperationException("StockBalanceModel должна быть заполнена!");
				return StockBalanceModel?.Balances.Where(x => 
					ProtectionTools.Nomenclatures.Select(n => n.Id)
						.Any( id => id == x.Position.Nomenclature.Id));
			}
		}
		public virtual IEnumerable<StockBalance> BestChoiceInStock {
			get {
				var bestChoice = InStock.Where(x => x.Amount > 0).ToList();
				return bestChoice;
			}
		}
		
		#endregion
		
		#region Граф

		public virtual IssueGraph Graph { get; set; }
		
		public virtual int Issued(DateTime onDate) => Graph.AmountAtEndOfDay(onDate);
		
		/// <summary>
		/// Необходимое к выдаче количество.
		/// </summary>
		public virtual int CalculateRequiredIssue(BaseParameters parameters, DateTime onDate) {
			if(Graph == null)
				throw new NullReferenceException($"Перед выполнением расчета {nameof(CalculateRequiredIssue)}, Graph должен быть заполнен!");
			
			return Math.Max(0, Amount - Graph.UsedAmountAtEndOfDay(onDate.AddDays(parameters.ColDayAheadOfShedule)));
		}

		/// <summary>
		/// Обновляет данные о выданном.
		/// </summary>
		/// <returns>Наличие изменений</returns>
		public virtual void Update(IUnitOfWork uow) {
			if(Id == 0)
				Graph = new IssueGraph();
			else {
				var query = uow.Session.QueryOver<DutyNormIssueOperation>()
					.Where(o => o.DutyNorm == DutyNorm && o.ProtectionTools == ProtectionTools);
				Graph = new IssueGraph(query.List<IGraphIssueOperation>());
			}
			NextIssue = CalculateNextIssue();
			OnPropertyChanged(nameof(Issued));
		}

		/// <summary>
		/// Рассчитывает дату следующей выдачи.
		/// </summary>
		public virtual DateTime? CalculateNextIssue() {
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

			if(wantIssue == default(DateTime))
				wantIssue = DutyNorm.DateFrom ?? DateTime.Now;
			return wantIssue;
		}

		#endregion
		
		#region Методы и расчётные свойства для view
		public virtual string Title => $@"{Amount} {ProtectionTools?.Type?.Units?.MakeAmountShortStr(Amount)}
			 ""{ProtectionTools.Name}"" на {PeriodCount} {PeriodText}";
		public virtual double AmountPerYear
		{
			get{
				double years = -1;
				switch(NormPeriod)
				{
					case DutyNormPeriodType.Year:
						years = PeriodCount;
						break;
					case DutyNormPeriodType.Month:
						years = (double)PeriodCount / 12;
						break;
					case DutyNormPeriodType.Wearout:
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
					case DutyNormPeriodType.Year:
						return PeriodCount * 12;
					case DutyNormPeriodType.Month:
						return PeriodCount;
					case DutyNormPeriodType.Wearout:
						return 0;
				}
				return -1;
			}
		}

		public virtual string PeriodText{
			get{
				switch(NormPeriod) {
					case DutyNormPeriodType.Year:
						return NumberToTextRus.FormatCase (PeriodCount, "год", "года", "лет");
					case DutyNormPeriodType.Month:
						return NumberToTextRus.FormatCase (PeriodCount, "месяц", "месяца", "месяцев");
					case DutyNormPeriodType.Wearout:
						return "До износа";
					default:
						return String.Empty;
				}
			}
		}
		
		public virtual string AmountColor {
			get {
				var amount = Issued(DateTime.Today);
				if (Amount < amount)
					return "blue";
				if (Amount > amount)
					return "orange";
				if (amount == 0)
					return "red";
				else
					return "black";
			}
		}

		public virtual string NextIssueColor {
			get {
				if(DateTime.Today >= NextIssue)
					return "darkred";
				if(DateTime.Today > NextIssue?.AddDays(10))
					return "orange";
				else
					return "black";
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
		#endregion

		/// <summary>
		/// Возвращает копию текущего объекта с привязкой к дежурной норме
		/// </summary>
		/// <returns>Копия текущего объекта DutyNormItem.</returns>
		/// <param name="DutyNorm">Дежурная норма, которой будет привязан возвращаемый объект DutyNormItem</param>
		public virtual DutyNormItem Copy(DutyNorm dutyNorm) 
		{
			DutyNormItem newDutyNormItem = new DutyNormItem();
			
			newDutyNormItem.dutyNorm = dutyNorm;
			newDutyNormItem.protectionTools = protectionTools;
			newDutyNormItem.amount = amount;
			newDutyNormItem.normPeriod = normPeriod;
			newDutyNormItem.periodCount = periodCount;
			newDutyNormItem.normParagraph = normParagraph;
			newDutyNormItem.comment = comment;
			newDutyNormItem.Graph = new IssueGraph();
			var issued = newDutyNormItem.Issued(DateTime.Now);
			newDutyNormItem.AmountUnitText(issued);
			
			return newDutyNormItem;
		}
	}
}
