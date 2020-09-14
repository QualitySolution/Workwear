using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Measurements;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "строки массовой выдачи",
	Nominative = "строка массовой выдачи")]
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
					ShoesSizeStd = SizeHelper.GetSizeStdCode(SizeHelper.GetDefaultSizeStd(СlothesType.Shoes, sex));
					GlovesSizeStd = SizeHelper.GetSizeStdCode(SizeHelper.GetDefaultSizeStd(СlothesType.Gloves, sex));
					WinterShoesSizeStd = SizeHelper.GetSizeStdCode(SizeHelper.GetDefaultSizeStd(СlothesType.WinterShoes, sex));
					HeaddressSizeStd = SizeHelper.GetSizeStdCode(SizeHelper.GetDefaultSizeStd(СlothesType.Headgear, sex));
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
			set { 
				SetField(ref wearSizeStd, value, () => WearSizeStd);
				if(wearSizeStd == null || !SizeHelper.GetSizesListByStdCode(wearSizeStd, SizeHelper.GetExcludedSizeUseForEmployee()).Contains(WearSize))
					wearSize = null;
			}
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
			set { SetField(ref shoesSizeStd, value, () => ShoesSizeStd);
				if(shoesSizeStd == null || !SizeHelper.GetSizesListByStdCode(shoesSizeStd, SizeHelper.GetExcludedSizeUseForEmployee()).Contains(ShoesSize))
					ShoesSize = null;
			}
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
			set { SetField(ref winterShoesSizeStd, value, () => WinterShoesSizeStd);
				if(winterShoesSizeStd == null || !SizeHelper.GetSizesListByStdCode(winterShoesSizeStd, SizeHelper.GetExcludedSizeUseForEmployee()).Contains(winterShoesSize))
					WinterShoesSize = null;
			}
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
			set { SetField(ref headdressSizeStd, value, () => HeaddressSizeStd);
				if(headdressSizeStd == null || !SizeHelper.GetSizesListByStdCode(headdressSizeStd, SizeHelper.GetExcludedSizeUseForEmployee()).Contains(HeaddressSize))
					HeaddressSize = null;
			}
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
			set { SetField(ref glovesSizeStd, value, () => GlovesSizeStd);
				if(glovesSizeStd == null || !SizeHelper.GetSizesListByStdCode(glovesSizeStd, SizeHelper.GetExcludedSizeUseForEmployee()).Contains(GlovesSize))
					GlovesSize = null;
			}
		}

		string glovesSize;

		[Display(Name = "Размер перчаток")]
		public virtual string GlovesSize {
			get { return glovesSize; }
			set { SetField(ref glovesSize, value, () => GlovesSize); }
		}

		string mittensSizeStd;

		[Display(Name = "Стандарт размера рукавиц")]
		public virtual string MittensSizeStd {
			get { return mittensSizeStd; }
			set {
				SetField(ref mittensSizeStd, value, () => MittensSizeStd);
				if(mittensSizeStd == null || !SizeHelper.GetSizesListByStdCode(mittensSizeStd, SizeHelper.GetExcludedSizeUseForEmployee()).Contains(MittensSize))
					MittensSize = null;
			}
		}


		string mittensSize;

		[Display(Name = "Размер рукавиц")]
		public virtual string MittensSize {
			get { return mittensSize; }
			set { SetField(ref mittensSize, value); }
		}

		IList<WarehouseOperation> listWarehouseOperation = new List<WarehouseOperation>();
		public virtual IList<WarehouseOperation> ListWarehouseOperation {
			get { return listWarehouseOperation; }
			set { SetField(ref listWarehouseOperation, value, () => ListWarehouseOperation); }
		}

		#endregion

		#region Пробросы

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
		#endregion

		#region Расчетные

		public virtual string Title => $"Сотрудник {EmployeeCard?.ShortName} в массовой выдаче";

		#endregion

		#region Методы
		public virtual SizePair GetSize(СlothesType? wearCategory)
		{
			switch(wearCategory) {
				case СlothesType.Wear:
					return new SizePair(WearSizeStd, WearSize);
				case СlothesType.Shoes:
					return new SizePair(ShoesSizeStd, ShoesSize);
				case СlothesType.WinterShoes:
					return new SizePair(WinterShoesSizeStd, WinterShoesSize);
				case СlothesType.Gloves:
					return new SizePair(GlovesSizeStd, GlovesSize);
				case СlothesType.Mittens:
					return new SizePair(SizeHelper.GetSizeStdCode(SizeStandartMittens.Rus), MittensSize);
				case СlothesType.Headgear:
					return new SizePair(HeaddressSizeStd, HeaddressSize);
				default:
					return null;
			}
		}

		public virtual SizePair GetGrow()
		{
			var growStd = SizeHelper.GetGrowthStandart(СlothesType.Wear, Sex, SizeUsePlace.Human);
			if(growStd == null || growStd.Length == 0)
				return null;
			return new SizePair(SizeHelper.GetSizeStdCode(growStd[0]), WearGrowth);
		}
		#endregion
	}
}
