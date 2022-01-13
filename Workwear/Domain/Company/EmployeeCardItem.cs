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
using workwear.Measurements;
using Workwear.Measurements;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Domain.Operations;

namespace workwear.Domain.Company
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки нормы карточки",
		Nominative = "строка нормы карточки")]
	public class EmployeeCardItem : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

		EmployeeCard employeeCard;

		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get { return employeeCard; }
			set { SetField (ref employeeCard, value, () => EmployeeCard); }
		}

		ProtectionTools protectionTools;

		[Display (Name = "Позиция")]
		public virtual ProtectionTools ProtectionTools {
			get { return protectionTools; }
			set { SetField (ref protectionTools, value, () => ProtectionTools); }
		}

		NormItem activeNormItem;

		[Display (Name = "Используемая строка нормы")]
		public virtual NormItem ActiveNormItem {
			get { return activeNormItem; }
			set { SetField (ref activeNormItem, value, () => ActiveNormItem); }
		}

		DateTime created;

		[Display (Name = "Создана")]
		public virtual DateTime Created {
			get { return created; }
			set { SetField (ref created, value, () => Created); }
		}

		int amount;

		[Display (Name = "Выданное количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		DateTime? lastIssue;

		[Display (Name = "Последняя выдача")]
		public virtual DateTime? LastIssue {
			get { return lastIssue; }
			set { SetField (ref lastIssue, value, () => LastIssue); }
		}

		DateTime? nextIssue;

		[Display (Name = "Следующая выдача")]
		public virtual DateTime? NextIssue {
			get { return nextIssue; }
			set { SetField (ref nextIssue, value, () => NextIssue); }
		}

		#endregion

		#region Не хранимое в базе значение


		IList<StockBalanceDTO> inStock;

		[Display (Name = "На складе")]
		public virtual IList<StockBalanceDTO> InStock {
			get { return inStock; }
			set { SetField (ref inStock, value, () => InStock); }
		}

		public virtual EmployeeIssueOperation LastIssueOperation { get; set; }

		#endregion

		#region Расчетное

		public virtual string AmountColor {
			get{
				if(ActiveNormItem == null)
					return "Indigo";
				else if (ActiveNormItem.Amount == Amount)
					return "darkgreen";
				else if (ActiveNormItem.Amount < Amount)
					return "blue";
				else if (Amount == 0)
					return "red";
				else
					return "orange";
			}
		}
		
		public virtual string NextIssueColor(BaseParameters parameters) {
			if(DateTime.Today > NextIssue)
					return "red";
			if (DateTime.Today.AddDays(parameters.ColDayAheadOfShedule) > NextIssue)
				return "darkgreen";
			return "black";
		}

		public virtual string Title{
			get{ return String.Format ("Потребность сотрудника {3} в {0} - {1} на {2}", ProtectionTools.Name, ProtectionTools.GetAmountAndUnitsText(ActiveNormItem.Amount), ActiveNormItem.LifeText, EmployeeCard.ShortName);
			}
		}

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
			.OrderBy(x => ProtectionTools.MatchedNomenclatures.TakeWhile(n => !n.IsSame(x.Nomenclature)).Count())
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
				var text = first.StockPosition.Title + " - " + ProtectionTools?.Type?.Units?.MakeAmountShortStr(first.Amount) ?? first.Amount.ToString();
				if(InStock.Count > 1)
					text += NumberToTextRus.FormatCase(InStock.Count - 1, " (еще {0} вариант)", " (еще {0} варианта)", " (еще {0} вариантов)");
				return text;
			}
		}


		#region Расчетное для View

		public virtual string AmountByNormText => ProtectionTools?.Type?.Units?.MakeAmountShortStr(ActiveNormItem?.Amount ?? 0) ?? ActiveNormItem?.Amount.ToString();
		public virtual string InStockText => ProtectionTools?.Type?.Units?.MakeAmountShortStr(InStock?.Sum(x => x.Amount) ?? 0) ?? InStock?.Sum(x => x.Amount).ToString();
		public virtual string AmountText => ProtectionTools?.Type?.Units?.MakeAmountShortStr(Amount) ?? Amount.ToString();
		public virtual string TonText => ActiveNormItem?.Norm?.TONParagraph;
		public virtual string NormLifeText => ActiveNormItem?.LifeText;

		#endregion

		public EmployeeCardItem ()
		{
		}

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
			else 
				return ActiveNormItem.Amount - Amount;
		}

		public virtual bool MatcheStockPosition(StockPosition stockPosition)
		{
			if(!ProtectionTools.MatchedNomenclatures.Any(n => n.Id == stockPosition.Nomenclature.Id))
				return false;

			var wearCategory = stockPosition.Nomenclature.Type.WearCategory;

			if(wearCategory == null)
				return true;
			
			var sexMatching = stockPosition.Nomenclature.MatchingEmployeeSex(EmployeeCard.Sex);
			if (!SizeHelper.HasСlothesSizeStd(wearCategory.Value) || sexMatching == false)
				return sexMatching;

			var employeeSize = EmployeeCard.GetSize(wearCategory.Value);
			if(employeeSize == null || String.IsNullOrEmpty(employeeSize.Size) || String.IsNullOrEmpty(employeeSize.StandardCode)) {
				logger.Warn("В карточке сотрудника не указан размер для спецодежды типа <{0}>.", ProtectionTools.Name);
				return false;
			}

			var validSizes = SizeHelper.MatchSize(employeeSize, SizeUsePlace.Сlothes);
			if(!validSizes.Any(s => s.StandardCode == stockPosition.Nomenclature.SizeStd && s.Size == stockPosition.Size))
				return false;

			if(!String.IsNullOrEmpty(stockPosition.Growth) && SizeHelper.HasGrowthStandart(wearCategory.Value)) {
				var validGrowths = SizeHelper.MatchGrow(EmployeeCard.WearGrowth, SizeUsePlace.Сlothes);
				if(!validGrowths.Any(s => s.Size == stockPosition.Growth))
					return false;
			}
			return true;
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

			if(NextIssue != wantIssue)
			{
				NextIssue = wantIssue;
				uow.Save (this);
			}
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

