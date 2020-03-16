using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using workwear.Domain.Company;
using workwear.Measurements;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "строки перевода массовой выдачи",
	Nominative = "строка перевода массовой выдачи")]
	public class MassExpenseEmployee : PropertyChangedBase, IDomainObject
	{
		public MassExpenseEmployee()
		{
		}


		public MassExpenseEmployee(MassExpense doc)
		{
			this.documentMassExpense = doc;
		}


		public virtual int Id { get; set; }

		private MassExpense documentMassExpense;

		[Display(Name = "Документ")]
		public virtual MassExpense DocumentMassExpense {
			get { return documentMassExpense; }
			set { SetField(ref documentMassExpense, value); }
		}

		EmployeeCard employeeCard;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get { return employeeCard; }
			set { SetField(ref employeeCard, value, () => EmployeeCard); }
		}

		Sex sex;

			[Display(Name = "Пол")]
			public virtual Sex Sex {
				get { return sex; }
				set {
				SetField(ref sex, value, () => Sex);
				if(Sex != Sex.None) {
					WearSizeStd = SizeHelper.GetSizeStdCode(SizeHelper.GetDefaultSizeStd(СlothesType.Wear, sex));
					shoesSizeStd = SizeHelper.GetSizeStdCode(SizeHelper.GetDefaultSizeStd(СlothesType.Shoes, sex));
				}
				}
			}


		#region Размеры одежды

		string wearGrowth;

		[Display(Name = "Рост одежды")]
		public virtual string WearGrowth {
			get { return wearGrowth; }
			set { SetField(ref wearGrowth, value, () => WearGrowth); }
		}

		string wearSizeStd;

		[Display(Name = "Стандарт размера одежды")]
		public virtual string WearSizeStd {
			get { return wearSizeStd; }
			set { SetField(ref wearSizeStd, value, () => WearSizeStd); }
		}

		string wearSize;

		[Display(Name = "Размер одежды")]
		public virtual string WearSize {
			get { return wearSize; }
			set { SetField(ref wearSize, value, () => WearSize); }
		}

		string shoesSizeStd;

		[Display(Name = "Стандарт размера обуви")]
		public virtual string ShoesSizeStd {
			get { return shoesSizeStd; }
			set { SetField(ref shoesSizeStd, value, () => ShoesSizeStd); }
		}

		string shoesSize;

		[Display(Name = "Размер обуви")]
		public virtual string ShoesSize {
			get { return shoesSize; }
			set { SetField(ref shoesSize, value, () => ShoesSize); }
		}

		string winterShoesSizeStd;

		[Display(Name = "Стандарт размера зимней обуви")]
		public virtual string WinterShoesSizeStd {
			get { return winterShoesSizeStd; }
			set { SetField(ref winterShoesSizeStd, value, () => WinterShoesSizeStd); }
		}

		string winterShoesSize;

		[Display(Name = "Размер зимней обуви")]
		public virtual string WinterShoesSize {
			get { return winterShoesSize; }
			set { SetField(ref winterShoesSize, value, () => WinterShoesSize); }
		}

		string headdressSizeStd;

		[Display(Name = "Стандарт размера головного убора")]
		public virtual string HeaddressSizeStd {
			get { return headdressSizeStd; }
			set { SetField(ref headdressSizeStd, value, () => HeaddressSizeStd); }
		}

		string headdressSize;

		[Display(Name = "Размер головного убора")]
		public virtual string HeaddressSize {
			get { return headdressSize; }
			set { SetField(ref headdressSize, value, () => HeaddressSize); }
		}

		string glovesSizeStd;

		[Display(Name = "Стандарт размера перчаток")]
		public virtual string GlovesSizeStd {
			get { return glovesSizeStd; }
			set { SetField(ref glovesSizeStd, value, () => GlovesSizeStd); }
		}

		string glovesSize;

		[Display(Name = "Размер перчаток")]
		public virtual string GlovesSize {
			get { return glovesSize; }
			set { SetField(ref glovesSize, value, () => GlovesSize); }
		}

		#endregion

		public virtual string FirstName {
			get {
				return EmployeeCard.FirstName;
			}
			set { EmployeeCard.FirstName = value; }
		}

		public virtual string LastName {
			get {
				return EmployeeCard.LastName;
			}
			set { EmployeeCard.LastName = value; }
		}

		public virtual string Patronymic {
			get {
				return EmployeeCard.Patronymic;
			}
			set { EmployeeCard.Patronymic = value; }
		}

	}
}
