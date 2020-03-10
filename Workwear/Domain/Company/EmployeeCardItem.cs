﻿using System;
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

		ItemsType item;

		[Display (Name = "Позиция")]
		public virtual ItemsType Item {
			get { return item; }
			set { SetField (ref item, value, () => Item); }
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

		IList<Nomenclature> matchedNomenclature;

		[Display(Name = "Подходящие номенклатуры")]
		public virtual IList<Nomenclature> MatchedNomenclature {
			get { return matchedNomenclature; }
			set { SetField(ref matchedNomenclature, value, () => MatchedNomenclature); }
		}

		IList<StockBalanceDTO> inStock;

		[Display (Name = "На складе")]
		public virtual IList<StockBalanceDTO> InStock {
			get { return inStock; }
			set { SetField (ref inStock, value, () => InStock); }
		}

		#endregion

		#region Расчетное

		public virtual string AmountColor {
			get{
				if (ActiveNormItem.Amount == Amount)
					return "darkgreen";
				else if (ActiveNormItem.Amount < Amount)
					return "blue";
				else if (Amount == 0)
					return "red";
				else
					return "orange";
			}
		}

		public virtual int NeededAmount{
			get{ return ActiveNormItem.Amount - Amount;	}
		}

		public virtual string Title{
			get{ return String.Format ("Потребность сотрудника {3} в {0} - {1} на {2}", Item.Name, Item.Units.MakeAmountShortStr (ActiveNormItem.Amount), ActiveNormItem.LifeText, EmployeeCard.ShortName);
			}
		}

		public virtual StockStateInfo InStockState {
			get {
				if(InStock == null || MatchedNomenclature == null)
					return StockStateInfo.NotLoaded;

				if(!matchedNomenclature.Any())
					return StockStateInfo.UnknownNomenclature;

				if(InStock.Any(x => x.Amount >= NeededAmount))
					return StockStateInfo.Enough;

				if(InStock.Sum(x => x.Amount) <= 0)
					return StockStateInfo.OutOfStock;

				return StockStateInfo.NotEnough;
			}
		}

		public virtual IEnumerable<StockBalanceDTO> BestChoiceInStock => InStock
			.OrderBy(x => x.WearPercent)
			.ThenByDescending(x => x.Amount);

		public virtual string MatchedNomenclatureShortText {
			get {
				if(InStockState == StockStateInfo.UnknownNomenclature)
					return "нет подходящей";

				if(InStock == null || InStock.Count == 0)
					return String.Empty;

				var first = BestChoiceInStock.First();
				var text = first.StockPosition.Title + " - " + Item.Units.MakeAmountShortStr(first.Amount);
				if(InStock.Count > 1)
					text += NumberToTextRus.FormatCase(InStock.Count - 1, " (еще {0} вариант)", " (еще {0} варианта)", " (еще {0} вариантов)");
				return text;
			}
		}

		#endregion

		public EmployeeCardItem ()
		{
		}

		public EmployeeCardItem (EmployeeCard employee, NormItem normItem)
		{
			EmployeeCard = employee;
			ActiveNormItem = normItem;
			Item = normItem.Item;
			NextIssue = Created = DateTime.Today;
		}

		public virtual bool MatcheStockPosition(StockPosition stockPosition)
		{
			if(!MatchedNomenclature.Any(n => n.Id == stockPosition.Nomenclature.Id))
				return false;

			if(Item.WearCategory == null || !SizeHelper.HasСlothesSizeStd(Item.WearCategory.Value))
				return true;

			var employeeSize = EmployeeCard.GetSize(Item.WearCategory.Value);
			if(employeeSize == null || String.IsNullOrEmpty(employeeSize.Size) || String.IsNullOrEmpty(employeeSize.StandardCode)) {
				logger.Warn("В карточке сотрудника не указан размер для спецодежды типа <{0}>.", Item.Name);
				return false;
			}

			var validSizes = SizeHelper.MatchSize(employeeSize, SizeUsePlace.Сlothes);
			if(!validSizes.Any(s => s.StandardCode == stockPosition.Nomenclature.SizeStd && s.Size == stockPosition.Size))
				return false;

			if(SizeHelper.HasGrowthStandart(Item.WearCategory.Value)) {
				var growStds = SizeHelper.GetGrowthStandart(Item.WearCategory.Value, EmployeeCard.Sex, SizeUsePlace.Сlothes);
				var validGrowths = SizeHelper.MatchGrow(growStds, EmployeeCard.WearGrowth, SizeUsePlace.Сlothes);
				if(!validGrowths.Any(s => s.StandardCode == stockPosition.Nomenclature.WearGrowthStd && s.Size == stockPosition.Growth))
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

		#region Зазоры для тестирования

		protected internal virtual IssueGraph GetIssueGraphForItem(IUnitOfWork uow)
		{
			return IssueGraph.MakeIssueGraph(uow, EmployeeCard, Item);
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

