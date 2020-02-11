using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Domain.Stock
{
	public class MassExpenseIssuing : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Nomenclature nomenclature;

		[Display(Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value); }
		}

		int amount;

		[Display(Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField(ref amount, value); }
		}

		MassExpense massExpense;

		public virtual MassExpense MassExpense {
			get { return massExpense; }
			set { SetField(ref massExpense, value); }
		}



	}
}
