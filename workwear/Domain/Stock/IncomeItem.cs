using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки прихода",
		Nominative = "строка прихода")]
	public class IncomeItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Income document;

		[Display(Name = "Документ")]
		public virtual Income Document {
			get { return document; }
			set { SetField(ref document, value); }
		}


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
		[Range(0.0, 1.0)]
		public virtual decimal LifePercent {
			get { return lifePercent; }
			set { SetField (ref lifePercent, value.Clamp(0, 1), () => LifePercent); }
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

		private EmployeeIssueOperation employeeIssueOperation;

		[Display(Name = "Операция возврата от сотрудника")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get { return employeeIssueOperation; }
			set { SetField(ref employeeIssueOperation, value); }
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

		#region Не сохраняемые в базу свойства

		private string buhDocument;

		[Display(Name = "Документ бухгалтерского учета")]
		//В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		public virtual string BuhDocument
		{
			get { return buhDocument ?? EmployeeIssueOperation?.BuhDocument; }
			set { SetField(ref buhDocument, value); }
		}

		#endregion

		protected IncomeItem ()
		{
		}

		public IncomeItem(Income income)
		{
			document = income;
		}

		#region Функции

		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			if(Document.Operation == IncomeOperations.Return) {
				if(EmployeeIssueOperation == null)
					EmployeeIssueOperation = new EmployeeIssueOperation();
				EmployeeIssueOperation.Update(uow, askUser, this);
			}
			else if(EmployeeIssueOperation != null) {
				uow.Delete(EmployeeIssueOperation);
				EmployeeIssueOperation = null;
			}
		}

		#endregion

	}
}

