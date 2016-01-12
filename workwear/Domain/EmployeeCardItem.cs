using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QSOrmProject;
using workwear.Domain.Stock;
using Gamma.Utilities;

namespace workwear.Domain
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

		bool requiredByNorm;

		[Display (Name = "Активная норма")]
		public virtual bool RequiredByNorm {
			get { return requiredByNorm; }
			set { SetField (ref requiredByNorm, value, () => RequiredByNorm); }
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

			if (neededCount <= 0)
			{
				logger.Debug ("Нет необходимости в выдаче <{0}>, пропускаем подбор...", Item.Name);
				return;
			}

			var nomenclatures = StockRepository.MatchNomenclaturesBySize (uow, Item, EmployeeCard);
			if(nomenclatures == null || nomenclatures.Count == 0)
			{
				logger.Warn ("Подходящая по размерам номенклатура, для типа <{0}> не найдена.", Item.Name);
				InStockState = StockStateInfo.UnknownNomenclature;
				return;
			}
			var stock = StockRepository.BalanceInStockDetail (uow, nomenclatures);

			var suggested = stock.First (s => s.Amount >= neededCount && s.Life == 1);
			if (suggested == null) {
				logger.Debug ("Достаточного количества новых <{0}> на складе не найдено.", Item.Name);
				suggested = stock.Aggregate ((agr, cur) => cur.Amount > agr.Amount ? cur : agr);
			}	
				
			if(DomainHelper.EqualDomainObjects (MatchedNomenclature, suggested))
			{
				logger.Debug ("Только обновляем количество на складе <{0}> -> <{1}>", InStock, suggested.Amount);
			}
			else
			{
				logger.Debug ("Изменяем подобранную номенклатуру <{0}> -> <{1}>", 
					MatchedNomenclature != null ? MatchedNomenclature.Name : String.Empty,
					suggested.Nomenclature);
				MatchedNomenclature = suggested.Nomenclature;
			}

			SetInStockAmount (suggested.Amount);
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

