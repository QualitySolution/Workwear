using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using workwear.Domain.Stock;

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
			var nomenclatures = StockRepository.MatchNomenclaturesBySize (uow, Item, EmployeeCard);
			if(nomenclatures == null || nomenclatures.Count == 0)
			{
				logger.Warn ("Подходящая по размерам номенклатура, для типа <{0}> не найдена.", Item.Name);
				return;
			}
			var stock = StockRepository.BalanceInStockDetail (uow, nomenclatures);
			Console.WriteLine (stock.Count);
		}
	}
}

