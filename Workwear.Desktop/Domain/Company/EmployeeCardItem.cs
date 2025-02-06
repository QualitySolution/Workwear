using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities.Dates;
using QS.Utilities;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;
using Workwear.Tools;

namespace Workwear.Domain.Company
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки нормы карточки",
		Nominative = "строка нормы карточки",
		Genitive = "строки нормы карточки"
		)]
	[HistoryTrace]
	public class EmployeeCardItem : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства
		public virtual int Id { get; set; }

		private EmployeeCard employeeCard;
		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get => employeeCard;
			set => SetField (ref employeeCard, value);
		}

		private ProtectionTools protectionTools;
		[Display (Name = "Позиция")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField (ref protectionTools, value);
		}

		private NormItem activeNormItem;
		[Display (Name = "Используемая строка нормы")]
		public virtual NormItem ActiveNormItem {
			get => activeNormItem;
			set => SetField (ref activeNormItem, value);
		}

		private DateTime created;
		[Display (Name = "Создана")]
		public virtual DateTime Created {
			get => created;
			set => SetField (ref created, value);
		}

		private DateTime? nextIssue;
		[Display (Name = "Следующая выдача")]
		public virtual DateTime? NextIssue {
			get => nextIssue;
			set => SetField (ref nextIssue, value);
		}
		
		private string nextIssueAnnotation;
		[IgnoreHistoryTrace]
		[Display (Name = "Объяснение расчёта следующей выдачи")]
		public virtual string NextIssueAnnotation {
			get => ProtectionTools?.Dispenser ?? false ? String.Empty : nextIssueAnnotation;
			set => SetField (ref nextIssueAnnotation, value);
		}
		#endregion
		/// <summary>
		/// Получаем значения остатков на складе для подходящих позиций.
		/// ВНИМАНИЕ! StockBalanceModel должна быть заполнена!
		/// </summary>
		public virtual IEnumerable<StockBalance> InStock {
			get { 				
				if(StockBalanceModel == null)
					throw new InvalidOperationException("StockBalanceModel должна быть заполнена!");
				return StockBalanceModel?.Balances.Where(x => MatchStockPosition(x.Position));
			}
		}

		#region Модели
		public virtual StockBalanceModel StockBalanceModel { get; set; }
		public virtual IssueGraph Graph { get; set; }
		#endregion
		#region Расчетное
		public virtual EmployeeIssueOperation LastIssueOperation(DateTime onDate, BaseParameters baseParameters) => (EmployeeIssueOperation)LastIssued(onDate, baseParameters).LastOrDefault().item?.IssueOperation;
		public virtual string AmountColor {
			get {
				var amount = Issued(DateTime.Today);
				if(ActiveNormItem == null)
					return "Indigo";
				if (ActiveNormItem.Amount == amount)
					return "darkgreen";
				if (ActiveNormItem.Amount < amount)
					return "blue";
				if (amount == 0)
					return "red";
				else
					return "orange";
			}
		}
		public virtual string NextIssueColor(BaseParameters parameters)
		{
			if(DateTime.Today > NextIssue)
					return "red";
			if (DateTime.Today.AddDays(parameters.ColDayAheadOfShedule) > NextIssue)
				return "darkgreen";
			else
				return "black";
		}
		public virtual string Title =>
			$"Потребность сотрудника {EmployeeCard.ShortName} в {ProtectionTools.Name} - " +
			$"{ActiveNormItem.AmountText} на {ActiveNormItem.LifeText}";

		public virtual StockStateInfo InStockState {
			get {
				if(StockBalanceModel == null)
					return StockStateInfo.NotLoaded;

				if(!ProtectionTools.Nomenclatures.Any())
					return StockStateInfo.UnknownNomenclature;

				if(InStock.Any(x => x.Amount >= ActiveNormItem.Amount))
					return StockStateInfo.Enough;

				if(InStock.Sum(x => x.Amount) <= 0)
					return StockStateInfo.OutOfStock;

				return StockStateInfo.NotEnough;
			}
		}
		public virtual IEnumerable<StockBalance> BestChoiceInStock {
			get {
				var bestChoice = InStock.Where(x => x.Amount > 0).ToList();
				bestChoice?.Sort(new BestChoiceInStockComparer(ProtectionTools));
				return bestChoice;
			}
		}

		public virtual int Issued(DateTime onDate) => Graph.AmountAtEndOfDay(onDate);
		
		public virtual (DateTime date, int amount, int removed, GraphItem item)[] LastIssued(DateTime onDate, BaseParameters baseParameters) {
			if(!Graph.Intervals.Any())
				return Array.Empty<(DateTime date, int amount, int removed, GraphItem item)>();
			Dictionary<int, (DateTime date, int amount, int removed, GraphItem item)> showed = new Dictionary<int, (DateTime date, int amount, int removed, GraphItem item)>();
			
			foreach(var interval in Graph.OrderedIntervalsReverse) {
				if(interval.StartDate <= onDate 
				   && showed.Count == 1 
				   && showed.First().Value.amount == ((EmployeeIssueOperation)showed.First().Value.item.IssueOperation).NormItem?.Amount
				                                      && interval.AmountAtEndOfDay(showed.First().Value.date.AddDays(baseParameters.ColDayAheadOfShedule), showed.First().Value.item.IssueOperation) == 0 )
					break;
				
				foreach(var item in interval.ActiveIssues) {
					if(showed.ContainsKey(item.IssueOperation.Id))
						continue;
					showed.Add(item.IssueOperation.Id, (item.IssueOperation.OperationTime, item.IssueOperation.Issued, item.WriteOffOperations.Sum(x => x.Returned), item));
				}
				if(interval.StartDate <= onDate && showed.Any())
					break;
			}

			return showed.Values.OrderBy(x => x.date).ToArray();
		}
		#endregion
		#region Расчетное для View
		public virtual string MatchedNomenclatureShortText {
			get {
				if(ProtectionTools?.Dispenser ?? false)
					return String.Empty; 
				if(InStockState == StockStateInfo.UnknownNomenclature)
					return "нет подходящей";

				if(StockBalanceModel == null || !InStock.Any() || !BestChoiceInStock.Any())
					return String.Empty;

				var first = BestChoiceInStock.First();
				var text = first.Position.Title + " - " + 
				           (ProtectionTools?.Type?.Units?.MakeAmountShortStr(first.Amount) ?? first.Amount.ToString());
				var inStockCount = InStock.Count();
				if(inStockCount > 1)
					text += NumberToTextRus.FormatCase(
						inStockCount - 1, " (еще {0} вариант)", " (еще {0} варианта)", " (еще {0} вариантов)");
				return text;
			}
		}
		public virtual string AmountByNormText => ActiveNormItem?.AmountText;
		public virtual string InStockText {
			get {
				if(ProtectionTools?.Dispenser ?? false)
					return String.Empty;
				if(InStockState == StockStateInfo.NotLoaded)
					return "Нет данных";
				return ProtectionTools?.Type?.Units?.MakeAmountShortStr(BestChoiceInStock.Sum(x => x.Amount)) ??
					  BestChoiceInStock.Sum(x => x.Amount).ToString();
			}
		}

		public virtual string AmountText => ProtectionTools?.Dispenser ?? false ? String.Empty : ProtectionTools?.Type?.Units?.MakeAmountShortStr(Issued(DateTime.Today)) ?? Issued(DateTime.Today).ToString();

		public virtual string DelayText => ProtectionTools?.Dispenser ?? false ? String.Empty : 
			(NextIssue.HasValue && NextIssue.Value < DateTime.Today)
				? NumberToTextRus.FormatCase((int)(DateTime.Today - NextIssue.Value).TotalDays, "{0} день", "{0} дня", "{0} дней")
				: String.Empty;
		public virtual string NextIssueText => ProtectionTools?.Dispenser ?? false ? String.Empty : $"{NextIssue:d}";
		public virtual string TonText => ActiveNormItem?.Norm?.TONParagraph;
		public virtual string NormLifeText => ActiveNormItem?.LifeText;
		#endregion
		public EmployeeCardItem () { }
		public EmployeeCardItem (EmployeeCard employee, NormItem normItem)
		{
			EmployeeCard = employee;
			ActiveNormItem = normItem;
			ProtectionTools = normItem.ProtectionTools;
			NextIssue = Created = DateTime.Today;
		}

		#region Methods
		/// <summary>
		/// Получить необходимое к выдаче количество.
		/// </summary>
		public virtual int CalculateRequiredIssue(BaseParameters parameters, DateTime onDate) {
			if(Graph == null)
				throw new NullReferenceException("Перед выполнением расчета CalculateRequiredIssue, Graph должен быть заполнен!");
			
			if(ActiveNormItem == null)
				return 0;
			if(employeeCard.DismissDate != null)
				return 0;
			if(employeeCard.OnVacation(onDate))
				return 0;
			if (ActiveNormItem.NormCondition?.IssuanceStart != null && ActiveNormItem.NormCondition?.IssuanceEnd != null) {
				var nextPeriod = ActiveNormItem.NormCondition.CalculateCurrentPeriod(onDate);
				if (onDate < nextPeriod.Begin)
					return 0;
			}

			return Math.Max(0, ActiveNormItem.Amount - Graph.UsedAmountAtEndOfDay(onDate.AddDays(parameters.ColDayAheadOfShedule)));
		}

		public virtual bool MatchStockPosition(StockPosition stockPosition) {
			if (ProtectionTools.Nomenclatures.All(n => n.Id != stockPosition.Nomenclature.Id))
				return false;
			if (stockPosition.Nomenclature.MatchingEmployeeSex(EmployeeCard.Sex) == false)
				return false;

			var employeeSize = EmployeeCard.Sizes.FirstOrDefault(x => x.SizeType == stockPosition.WearSize?.SizeType)?.Size;

			if (employeeSize is null && stockPosition.WearSize != null) 
				return false;

			if (employeeSize != null && stockPosition.WearSize != null) {
				var suitableStockPositionSize = stockPosition.WearSize.SuitableSizes
					.Union(stockPosition.WearSize.SizesWhereIsThisSizeAsSuitable)
					.ToList();
				
				suitableStockPositionSize.Add(stockPosition.WearSize);

				if (!suitableStockPositionSize.Contains(employeeSize)) return false;
			}
			
			if(stockPosition.Height is null) return true;
			
			var employeeHeight = employeeCard.Sizes
				.FirstOrDefault(x => x.SizeType == stockPosition.Height.SizeType)?.Size;
			if (employeeHeight is null)
				return false;

			var suitableStockPositionHeights = stockPosition.Height.SuitableSizes
					.Union(stockPosition.Height.SizesWhereIsThisSizeAsSuitable)
					.ToList();
			
			suitableStockPositionHeights.Add(stockPosition.Height);
			return suitableStockPositionHeights.Contains(employeeHeight);
		}

		/// <summary>
		/// Обновляет дату следующей выдачи.
		/// Перед вызовом метода Graph должен быть заполнен!
		/// </summary>
		/// <param name="uow">Необходим для сохранения строки.</param>
		public virtual void UpdateNextIssue(IUnitOfWork uow) {
			if(Graph == null)
				throw new NullReferenceException("Перед выполнением расчета UpdateNextIssue, Graph должен быть заполнен!");
			DateTime? wantIssue = new DateTime();
			NextIssueAnnotation = null;
			if(Graph.Intervals.Any())
			{
				var listReverse = Graph.Intervals.OrderByDescending(x => x.StartDate).ToList();
				var lastInterval = listReverse.First();
				if(lastInterval.CurrentCount >= ActiveNormItem.Amount) {
					//Нет автосписания, следующая выдача чисто информативно проставляется по сроку носки
					var expiredByNorm = lastInterval.ActiveItems.Where(x => ((EmployeeIssueOperation)x.IssueOperation).ExpiryByNorm != null);
					if(expiredByNorm.Any())
						wantIssue = expiredByNorm.Max(x => ((EmployeeIssueOperation)x.IssueOperation).ExpiryByNorm.Value);
					else
						wantIssue = null;
				}
				else {
					//Ищем первый с конца интервал где не хватает выданного до нормы.
					foreach(var interval in listReverse) {
						if (interval.CurrentCount < ActiveNormItem.Amount)
							wantIssue = interval.StartDate;
						else
							break;
					}
				}
			}
			if(wantIssue == default(DateTime)) {
				wantIssue = Created.Date;
			}

			//Сдвигаем дату следующего получения на конец отпуска
			if(EmployeeCard.Vacations.Any(v => v.BeginDate <= wantIssue && v.EndDate >= wantIssue)) {
				var ranges = EmployeeCard.Vacations.Select(v => new DateRange(v.BeginDate, v.EndDate));
				var wearTime = new DateRange(DateTime.MinValue, DateTime.MaxValue);
				wearTime.ExcludedRanges.AddRange(ranges);
				var moveTo = wearTime.FindEndOfExclusion(wantIssue.Value);
				if(moveTo != null){
					wantIssue = moveTo.Value.AddDays(1);
					NextIssueAnnotation = "Дата выдачи перенесена на конец отпуска";
				}
			}
			
			if (wantIssue != null && ActiveNormItem?.NormCondition?.IssuanceStart != null && ActiveNormItem?.NormCondition?.IssuanceEnd != null) {
				var nextPeriod = ActiveNormItem.NormCondition.CalculateCurrentPeriod(wantIssue.Value);
				if (wantIssue < nextPeriod.Begin){
					wantIssue = nextPeriod.Begin;
					NextIssueAnnotation = $"Дата перенесена по условию нормы: {ActiveNormItem.NormCondition.Name}";
				}
			}

			if(NextIssue != wantIssue && wantIssue != default(DateTime)) {
				NextIssue = wantIssue;
				uow?.Save (this);
			}

			if(NextIssue < ActiveNormItem.Norm.DateFrom && ActiveNormItem.NormPeriod != NormPeriodType.Wearout && ActiveNormItem.NormPeriod != NormPeriodType.Duty){
				NextIssue = ActiveNormItem.Norm.DateFrom;
			}
			if(ActiveNormItem.NormPeriod == NormPeriodType.Wearout)
				NextIssueAnnotation = $"У строки нормы указан период - до износа";
			if(ActiveNormItem.NormPeriod == NormPeriodType.Duty)
				NextIssueAnnotation = $"У строки нормы указан период - дежурный";
		}
		#endregion
	}

	public enum StockStateInfo{
		[ColorName("gray")]
		NotLoaded,
		[ColorName("orange")]
		UnknownNomenclature,
		[ColorName("green")]
		Enough,
		[ColorName("blue")]
		NotEnough,
		[ColorName("red")]
		OutOfStock,
	}

	public class BestChoiceInStockComparer : IComparer<StockBalance> {
		private readonly ProtectionTools protectionTools;
		public BestChoiceInStockComparer(ProtectionTools protectionTools) => 
			this.protectionTools = protectionTools;
		//Сортируем позиции по следующим критериям
		//Сначала берем позицию более приоритетного собственника
		//Была выбрана номенклатура прямо указанная в номенклатуре нормы, а уже затем аналоги. (Возможно текущий код не совсем это реализует. А привязывается к порядку номенклатур, скорей всего это не логично, наверно надо будет улучшить.)
		//Далее смотрим чтобы был меньший процент износа, то есть выдавалась новая.
		//Далее выдаем ту которой больше на складе, чтобы оставалось больше разнообразия(решение сомнительное, но какое есть)
		public int Compare(StockBalance x, StockBalance y) {
			if(x is null || y is null)
				throw new ArgumentNullException();
			if(x.Position.Owner?.Priority != y.Position.Owner?.Priority)
				return (y.Position.Owner?.Priority ?? 0).CompareTo(x.Position.Owner?.Priority ?? 0);
			var xMatchedNomenclature = protectionTools.Nomenclatures.TakeWhile(n => !n.IsSame(x.Position.Nomenclature)).Count();
			var yMatchedNomenclature = protectionTools.Nomenclatures.TakeWhile(n => !n.IsSame(y.Position.Nomenclature)).Count();
			if(xMatchedNomenclature != yMatchedNomenclature)
				return xMatchedNomenclature.CompareTo(yMatchedNomenclature);
			if(x.Position.WearPercent != y.Position.WearPercent)
				return x.Position.WearPercent.CompareTo(y.Position.WearPercent);
			if(x.Amount != y.Amount)
				return y.Amount.CompareTo(x.Amount);
			return 0;
		}
	}
}

