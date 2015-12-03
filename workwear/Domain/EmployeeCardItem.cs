using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace workwear.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки нормы карточки",
		Nominative = "строка нормы карточки")]
	public class EmployeeCardItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

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

		int amount;

		[Display (Name = "Выданное количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		DateTime lastIssue;

		[Display (Name = "Последняя выдача")]
		public virtual DateTime LastIssue {
			get { return lastIssue; }
			set { SetField (ref lastIssue, value, () => LastIssue); }
		}

		DateTime nextIssue;

		[Display (Name = "Следующая выдача")]
		public virtual DateTime NextIssue {
			get { return nextIssue; }
			set { SetField (ref nextIssue, value, () => NextIssue); }
		}

		#endregion


		public EmployeeCardItem ()
		{
		}
	}
}

