using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания")]
	public class WriteoffItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Writeoff document;

		[Display(Name = "Документ списания")]
		public virtual Writeoff Document {
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

		[Display (Name = "Выдано в строке")]
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

		private EmployeeIssueOperation employeeIssueOperation;

		[Display(Name = "Операция списания с сотрудника")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get { return employeeIssueOperation; }
			set { SetField(ref employeeIssueOperation, value); }
		}

		#endregion

		#region Вычисляемые

		public virtual string LastOwnText{
			get{
				if (IncomeOn != null)
					return "склад";
				if(IssuedOn != null)
				{
					if (IssuedOn.ExpenseDoc.Operation == ExpenseOperations.Employee)
						return IssuedOn.ExpenseDoc.Employee.ShortName;

					if (IssuedOn.ExpenseDoc.Operation == ExpenseOperations.Object)
						return IssuedOn.ExpenseDoc.Subdivision.Name;
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

		public virtual decimal? WearPercent {
			get {
				if(IncomeOn != null)
					return 1 - IncomeOn.LifePercent;
				else if(IssuedOn?.EmployeeIssueOperation != null)
					return IssuedOn.EmployeeIssueOperation.CalculatePercentWear(document.Date);
				else
					return null; //FIXME Наверно нужно реализовать отображение процента для объектов тоже.

			}
		}

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

		protected WriteoffItem (){}

		public WriteoffItem(Writeoff writeOff)
		{
			document = writeOff;
		}

		#region Методы

		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			if(IssuedOn?.ExpenseDoc?.Operation == ExpenseOperations.Employee) {
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

