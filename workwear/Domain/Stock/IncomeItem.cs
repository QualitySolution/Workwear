using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace workwear.Domain.Stock
{
	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки прихода",
		Nominative = "строка прихода")]
	public class IncomeItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		Nomenclature nomenclature;

		[Display (Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		ExpenseItem issuedOn;

		[Display (Name = "Операция выдачи")]
		public virtual ExpenseItem IssuedOn {
			get { return issuedOn; }
			set { SetField (ref issuedOn, value, () => IssuedOn); }
		}

		decimal lifePercent;

		[Display (Name = "% состояния")]
		public virtual decimal LifePercent {
			get { return lifePercent; }
			set { SetField (ref lifePercent, value, () => LifePercent); }
		}

		int amount;

		[Display (Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		decimal cost;

		[Display (Name = "Цена")]
		[PropertyChangedAlso("Total")]
		public virtual decimal Cost {
			get { return cost; }
			set { SetField (ref cost, value, () => Cost); }
		}

		private string certificate;

		[Display(Name = "№ сертификата")]
		public virtual string Certificate
		{
			get { return certificate; }
			set { SetField(ref certificate, value, () => Certificate); }
		}

		#endregion

		#region Расчетные

		public virtual string Title {
			get { return String.Format ("Поступление на склад {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units.Name
			);}
		}

		public virtual decimal Total{ get{	return Cost * Amount; }}

		#endregion


		public IncomeItem ()
		{
		}

	}
}

