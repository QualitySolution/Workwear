using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "строки перевода массовой выдачи",
	Nominative = "строка перевода массовой выдачи")]
	public class MassExpenseNomenclature : PropertyChangedBase, IDomainObject
	{

		public virtual int Id { get; set; }

		private MassExpense documentMassExpense;

		[Display(Name = "Документ")]
		public virtual MassExpense DocumentMassExpense {
			get { return documentMassExpense; }
			set { SetField(ref documentMassExpense, value); }
		}

		public MassExpenseNomenclature()
		{
		}

		public MassExpenseNomenclature(MassExpense doc)
		{
			this.documentMassExpense = doc;
		}

		Nomenclature nomenclature;

		[Display(Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		int amount;

		[Display(Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get { return amount; }
			set { SetField(ref amount, value, () => Amount); }
		}

		EmployeeCard employeecard;

		public virtual EmployeeCard EmployeeCard {
			get { return employeecard; }
			set { SetField(ref employeecard, value, () => EmployeeCard); }
		}


		public virtual string Title {
			get {
				return String.Format("Массовая выдача со склада {0} ",
			  documentMassExpense.WarehouseFrom
		  );
			}
		}
	}
}
