using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace workwear.Domain.Stock
{
	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания")]
	public class WriteoffItem : PropertyChangedBase, IDomainObject
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

		IncomeItem incomeOn;

		[Display (Name = "Операция прихода")]
		public virtual IncomeItem IncomeOn {
			get { return incomeOn; }
			set { SetField (ref incomeOn, value, () => IncomeOn); }
		}

		int amount;

		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		#endregion

		public virtual string LastOwnText{
			get{
				if (IncomeOn != null)
					return "склад";
				if(IssuedOn != null)
				{
					if (IssuedOn.ExpenseDoc.Operation == ExpenseOperations.Employee)
						return IssuedOn.ExpenseDoc.EmployeeCard.ShortName;

					if (IssuedOn.ExpenseDoc.Operation == ExpenseOperations.Object)
						return IssuedOn.ExpenseDoc.Facility.Name;
				}
				return String.Empty;
			}
		}

		public virtual string Title {
			get { return String.Format ("Списание {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units.Name
			);}
		}

		public WriteoffItem ()
		{
		}

	}
}

