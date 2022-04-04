using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities;
using QS.Utilities.Dates;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using QS.HistoryLog;
using workwear.Domain.Sizes;

namespace workwear.Domain.Company
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

		private int amount;
		[Display (Name = "Выданное количество")]
		public virtual int Amount {
			get => amount;
			set => SetField (ref amount, value);
		}

		private DateTime? lastIssue;
		[Display (Name = "Последняя выдача")]
		public virtual DateTime? LastIssue {
			get => lastIssue;
			set => SetField (ref lastIssue, value);
		}

		private DateTime? nextIssue;
		[Display (Name = "Следующая выдача")]
		public virtual DateTime? NextIssue {
			get => nextIssue;
			set => SetField (ref nextIssue, value);
		}
		#endregion
		#region Не хранимое в базе значение
		private IList<StockBalanceDTO> inStock;
		[Display (Name = "На складе")]
		public virtual IList<StockBalanceDTO> InStock {
			get => inStock;
			set => SetField (ref inStock, value);
		}
		public virtual EmployeeIssueOperation LastIssueOperation { get; set; }
		#endregion
		#region Расчетное
		public virtual string AmountColor {
			get
			{
				if(ActiveNormItem == null)
					return "Indigo";
				if (ActiveNormItem.Amount == Amount)
					return "darkgreen";
				if (ActiveNormItem.Amount < Amount)
					return "blue";
				return Amount == 0 ? "red" : "orange";
			}
		}
		public virtual string NextIssueColor(BaseParameters parameters) {
			if(DateTime.Today > NextIssue)
					return "red";
			return DateTime.Today.AddDays(parameters.ColDayAheadOfShedule) > NextIssue ? "darkgreen" : "black";
		}
		public virtual string Title => 
			String.Format ("Потребность сотрудника {3} в {0} - {1} на {2}", 
				ProtectionTools.Name, 
				ProtectionTools.GetAmountAndUnitsText(ActiveNormItem.Amount), 
				ActiveNormItem.LifeText, 
				EmployeeCard.ShortName);

		public virtual StockStateInfo InStockState {
			get {
				if(InStock == null)
					return StockStateInfo.NotLoaded;

				if(!ProtectionTools.MatchedNomenclatures.Any())
					return StockStateInfo.UnknownNomenclature;

				if(InStock.Any(x => x.Amount >= ActiveNormItem.Amount))
					return StockStateInfo.Enough;

				if(InStock.Sum(x => x.Amount) <= 0)
					return StockStateInfo.OutOfStock;

				return StockStateInfo.NotEnough;
			}
		}
		public virtual IEnumerable<StockBalanceDTO> BestChoiceInStock => InStock
			.OrderBy(x => 
				ProtectionTools.MatchedNomenclatures.TakeWhile(n => !n.IsSame(x.Nomenclature)).Count())
			.ThenBy(x => x.WearPercent)
			.ThenByDescending(x => x.Amount);
		#endregion
		public virtual string MatchedNomenclatureShortText {
			get {
				if(InStockState == StockStateInfo.UnknownNomenclature)
					return "нет подходящей";

				if(InStock == null || InStock.Count == 0)
					return String.Empty;

				var first = BestChoiceInStock.First();
				var text = first.StockPosition.Title + " - " + 
				           (ProtectionTools?.Type?.Units?.MakeAmountShortStr(first.Amount) ?? first.Amount.ToString());
				if(InStock.Count > 1)
					text += NumberToTextRus.FormatCase(
						InStock.Count - 1, " (еще {0} вариант)", " (еще {0} варианта)", " (еще {0} вариантов)");
				return text;
			}
		}
		#region Расчетное для View
		public virtual string AmountByNormText => 
			ProtectionTools?.Type?.Units?.MakeAmountShortStr(ActiveNormItem?.Amount ?? 0) ?? ActiveNormItem?.Amount.ToString();
		public virtual string InStockText => 
			ProtectionTools?.Type?.Units?.MakeAmountShortStr(InStock?.Sum(x => x.Amount) ?? 0) ?? 
			InStock?.Sum(x => x.Amount).ToString();
		public virtual string AmountText => ProtectionTools?.Type?.Units?.MakeAmountShortStr(Amount) ?? Amount.ToString();
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
		/// Необходимое к выдачи количество.
		/// Внимание! Не корректно считает сложные ситуации, с неполной выдачей.
		/// </summary>
		public virtual int CalculateRequiredIssue(BaseParameters parameters)
		{
			if(NextIssue.HasValue && NextIssue.Value.AddDays(-parameters.ColDayAheadOfShedule) <= DateTime.Today)
				return ActiveNormItem.Amount;
			return ActiveNormItem.Amount - Amount;
		}

		public virtual bool MatcheStockPosition(StockPosition stockPosition) {
			if (ProtectionTools.MatchedNomenclatures.All(n => n.Id != stockPosition.Nomenclature.Id))
				return false;
			if (stockPosition.Nomenclature.MatchingEmployeeSex(EmployeeCard.Sex) == false)
				return false;

			var employeeSize = EmployeeCard.Sizes.Select(x => x.Size)
				.FirstOrDefault(x => x.SizeType == stockPosition.WearSize?.SizeType);

			if (employeeSize is null && stockPosition.WearSize != null) {
				logger.Warn("В карточке сотрудника не указан размер для спецодежды типа <{0}>.", ProtectionTools.Name);
				return false;
			}

			if (employeeSize != null && stockPosition.WearSize != null) {
				var suitableEmployeeSizes = employeeSize.SuitableSizes;
				var suitableStockPositionSize = stockPosition.WearSize.SuitableSizes;
				suitableEmployeeSizes.Add(employeeSize);
				suitableStockPositionSize.Add(stockPosition.WearSize);

				if (!suitableEmployeeSizes.Intersect(suitableStockPositionSize).Any()) return false;
			}

			var employeeHeight = employeeCard.Sizes
				.FirstOrDefault(x => x.SizeType.Category == Category.Height)?.Size;
			if (employeeHeight is null && stockPosition.Height != null) {
				logger.Warn("В карточке сотрудника не указан рост");
				return false;
			}

			if (employeeHeight is null || stockPosition.Height is null) return true;

			var suitableEmployeeHeights = employeeHeight.SuitableSizes;
			var suitableStockPositionHeights = stockPosition.Height.SuitableSizes;
			suitableEmployeeHeights.Add(employeeHeight);
			suitableStockPositionHeights.Add(stockPosition.Height);
			
			return suitableEmployeeHeights.Intersect(suitableStockPositionHeights).Any();
		}

		public virtual void UpdateNextIssue(IUnitOfWork uow)
		{
			IssueGraph graph = null;

			if(EmployeeCard.Id > 0) //Если карточка еще не разу не сохранялась. То и нечего запрашивать выдачи.
			{
				graph = GetIssueGraphForItem(uow);
			}

			DateTime? wantIssue = new DateTime();
			if(graph != null && graph.Intervals.Any())
			{
				var listReverse = graph.Intervals.OrderByDescending(x => x.StartDate).ToList();
				var lastInterval = listReverse.First();
				if(lastInterval.CurrentCount >= ActiveNormItem.Amount)
				{//Нет автосписания, следующая выдача чисто информативно проставляется по сроку носки
					var expiredByNorm = lastInterval.ActiveItems.Where(x => x.IssueOperation.ExpiryByNorm != null);
					if(expiredByNorm.Any())
						wantIssue = expiredByNorm.Max(x => x.IssueOperation.ExpiryByNorm.Value);
					else
						wantIssue = null;
				}
				else
				{
					//Ищем первый с конца интервал где не хватает выданного до нормы.
					foreach(var interval in listReverse)
					{
						if (interval.CurrentCount < ActiveNormItem.Amount)
							wantIssue = interval.StartDate;
						else
							break;
					}
				}
			}
			if(wantIssue == default(DateTime))
			{
				wantIssue = Created.Date;
			}

			//Сдвигаем дату следующего получения на конец отпуска
			if(EmployeeCard.Vacations.Any(v => v.BeginDate <= wantIssue && v.EndDate >= wantIssue)) {
				var ranges = EmployeeCard.Vacations.Select(v => new DateRange(v.BeginDate, v.EndDate));
				var wearTime = new DateRange(DateTime.MinValue, DateTime.MaxValue);
				wearTime.ExcludedRanges.AddRange(ranges);
				var moveTo = wearTime.FindEndOfExclusion(wantIssue.Value);
				if(moveTo != null)
					wantIssue = moveTo.Value.AddDays(1);
			}
			
			if (ActiveNormItem?.NormCondition?.IssuanceStart != null && ActiveNormItem?.NormCondition?.IssuanceEnd != null) {
				var nextPeriod = ActiveNormItem.NormCondition.CalculateCurrentPeriod(wantIssue.Value);
				if (wantIssue < nextPeriod.Begin)
					wantIssue = nextPeriod.Begin;
			}

			if(NextIssue != wantIssue)
			{
				NextIssue = wantIssue;
				uow.Save (this);
			}

			if(NextIssue < ActiveNormItem.Norm.DateFrom && ActiveNormItem.NormPeriod != NormPeriodType.Wearout)
				NextIssue = ActiveNormItem.Norm.DateFrom;
		}
		#endregion
		#region Зазоры для тестирования
		protected internal virtual IssueGraph GetIssueGraphForItem(IUnitOfWork uow)
		{
			return IssueGraph.MakeIssueGraph(uow, EmployeeCard, ProtectionTools);
		}

		#endregion
	}

	public enum StockStateInfo{
		[GtkColor("gray")]
		NotLoaded,
		[GtkColor("orange")]
		UnknownNomenclature,
		[GtkColor("green")]
		Enough,
		[GtkColor("blue")]
		NotEnough,
		[GtkColor("red")]
		OutOfStock,
	}
}

