using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.Domain.Organization
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

		Nomenclature matchedNomenclature;

		[Display (Name = "Подобранная номенклатура")]
		public virtual Nomenclature MatchedNomenclature {
			get { return matchedNomenclature; }
			set { SetField (ref matchedNomenclature, value, () => MatchedNomenclature); }
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

		int inStock;

		[Display (Name = "На складе")]
		public virtual int InStock {
			get { return inStock; }
			set { SetField (ref inStock, value, () => InStock); }
		}

		StockStateInfo inStockState;

		[Display (Name = "Статус")]
		public virtual StockStateInfo InStockState {
			get { return inStockState; }
			set { SetField (ref inStockState, value, () => InStockState); }
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

		public virtual void FindMatchedNomenclature(IUnitOfWork uow)
		{
			int neededCount = ActiveNormItem.Amount - Amount;

			var nomenclatures = StockRepository.MatchNomenclaturesBySize (uow, Item, EmployeeCard);
			if(nomenclatures == null || nomenclatures.Count == 0)
			{
				logger.Warn ("Подходящая по размерам номенклатура, для типа <{0}> не найдена.", Item.Name);
				MatchedNomenclature = null;
				InStockState = StockStateInfo.UnknownNomenclature;
				return;
			}
			var stock = StockRepository.BalanceInStockDetail (uow, nomenclatures);

			if(stock.Count == 0)
			{
				logger.Debug ("Подходящие номенклатуры на складе отсутствуют, выбираем любую...");
				MatchedNomenclature = nomenclatures.OrderBy (n => n.Id).Last ();
				SetInStockAmount (0);
				return;
			}

			var grouped = stock.GroupBy (s => s.NomenclatureId);

			var fullLife = grouped.FirstOrDefault (gp => gp.Where (s => s.Life == 1).Sum (s => s.Amount) >= neededCount);

			Nomenclature suggested = fullLife != null ? fullLife.First ().Nomenclature : null;

			if (suggested == null) {
				logger.Debug ("Достаточного количества новых <{0}> на складе не найдено.", Item.Name);

				int lastSum = -1;
				foreach(var gr in grouped)
				{
					int newSum = gr.Sum (s => s.Amount);
					if(newSum > lastSum)
					{
						suggested = gr.First ().Nomenclature;
						lastSum = newSum;
					}
				}
			}	
				
			int suggestedAmount = grouped.First (gp => gp.Key == suggested.Id).Sum (s => s.Amount);

			if(DomainHelper.EqualDomainObjects (MatchedNomenclature, suggested))
			{
				logger.Debug ("Только обновляем количество на складе <{0}> -> <{1}>", InStock, suggestedAmount);
			}
			else
			{
				logger.Debug ("Изменяем подобранную номенклатуру <{0}> -> <{1}>", 
					MatchedNomenclature != null ? MatchedNomenclature.Name : String.Empty,
					suggested);
				MatchedNomenclature = suggested;
			}

			SetInStockAmount (suggestedAmount);
		}

		public virtual void SetInStockAmount(int inStock)
		{
			int neededCount = ActiveNormItem.Amount - Amount;
			InStock = inStock;
			if (InStock >= neededCount)
				InStockState = StockStateInfo.Enough;
			else if(InStock == 0)
				InStockState = StockStateInfo.OutOfStock;
			else
				InStockState = StockStateInfo.NotEnough;
		}

		public virtual void UpdateNextIssue(IUnitOfWork uow)
		{
			IssueGraph graph = null;

			if(EmployeeCard.Id > 0) //Если карточка еще не разу не сохранялась. То и нечего запрашивать выдачи.
			{
				graph = GetIssueGraphForItem(uow);
			}

			DateTime wantIssue = new DateTime();
			if(graph != null && graph.Intervals.Any())
			{
				var listReverse = graph.Intervals.OrderByDescending(x => x.StartDate).ToList();
				var lastInterval = listReverse.First();
				if(lastInterval.CurrentCount >= ActiveNormItem.Amount)
				{//Нет автосписания, следующая выдача чисто информативно проставляется по сроку носки
					wantIssue = lastInterval.ActiveItems.Where(x => x.IssueOperation.ExpiryByNorm != null).Max(x => x.IssueOperation.ExpiryByNorm.Value);
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

			if(wantIssue < Created.Date)
			{
				wantIssue = Created.Date;
			}

			//Сдвигаем дату следующего получения на конец отпуска
			if (EmployeeCard.CurrentLeaveBegin.HasValue && EmployeeCard.CurrentLeaveEnd.HasValue
			    && wantIssue >= EmployeeCard.CurrentLeaveBegin.Value
			    && wantIssue <= EmployeeCard.CurrentLeaveEnd.Value)
				wantIssue = EmployeeCard.CurrentLeaveEnd.Value.AddDays(1);

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

