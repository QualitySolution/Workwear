using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.Domain.Organization
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
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

		public virtual void UpdateNextIssue(IUnitOfWork uow, ExpenseItem[] resaveItems)
		{
			ExpenseItem expenseItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			IList<ExpenseItem> expenseItems = new List<ExpenseItem>();

			if(EmployeeCard.Id > 0) //Если карточка еще не разу не сохранялась. То и нечего запрашивать выдачи.
			{
				expenseItems = uow.Session.QueryOver<ExpenseItem> (() => expenseItemAlias)
					.JoinQueryOver (ei => ei.ExpenseDoc)
					.Where (e => e.EmployeeCard == EmployeeCard)
					.JoinAlias (() => expenseItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where (() => nomenclatureAlias.Type.Id == Item.Id)
					.OrderBy (e => e.Date).Asc
					.List ();
			}

			var lastExpire = new DateTime();

			foreach(var expenseItem in expenseItems)
			{
				bool noChange = expenseItem.AutoWriteoffDate.HasValue && expenseItem.AutoWriteoffDate.Value < DateTime.Today && !resaveItems.Contains (expenseItem);

				var returned = uow.Session.QueryOver<IncomeItem> ()
					.Where (i => i.IssuedOn == expenseItem).List ();
				var writeoff = uow.Session.QueryOver<WriteoffItem> ()
					.Where (w => w.IssuedOn == expenseItem).List ();

				int realAmount = expenseItem.Amount - returned.Sum (r => r.Amount) - writeoff.Sum (w => w.Amount);

				DateTime virtualIssue = lastExpire > expenseItem.ExpenseDoc.Date ? lastExpire : expenseItem.ExpenseDoc.Date;

				DateTime newExpireDate = ActiveNormItem.CalculateExpireDate (virtualIssue, realAmount);

				if (newExpireDate > DateTime.Today)
					noChange = false;

				lastExpire = newExpireDate;
				if(!noChange && expenseItem.AutoWriteoffDate != (DateTime?)newExpireDate)
				{
					expenseItem.AutoWriteoffDate = newExpireDate;
					uow.Save (expenseItem);
				}
			}

			if(lastExpire != default(DateTime))
			{
				//Сдвигаем дату следующего получения на конец дикретного отпуска
				if (EmployeeCard.MaternityLeaveBegin.HasValue && EmployeeCard.MaternityLeaveEnd.HasValue
				    && lastExpire >= EmployeeCard.MaternityLeaveBegin.Value
				    && lastExpire <= EmployeeCard.MaternityLeaveEnd.Value)
					lastExpire = EmployeeCard.MaternityLeaveEnd.Value.AddDays(1);

				if(NextIssue != lastExpire)
				{
					NextIssue = lastExpire;
					uow.Save (this);
				}
			}
		}
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

