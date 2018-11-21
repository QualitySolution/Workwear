using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using workwear.Domain.Organization;
using QS.DomainModel.Entity;
using workwear.Domain.Operations;
using QS.DomainModel.UoW;
using workwear.Domain.Regulations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи",
		Nominative = "строка выдачи")]
	public class ExpenseItem : PropertyChangedBase, IDomainObject
	{
		#region Сохраняемые свойства

		public virtual int Id { get; set; }

		Expense expenseDoc;

		[Display (Name = "Документ")]
		public virtual Expense ExpenseDoc {
			get { return expenseDoc; }
			set { SetField (ref expenseDoc, value, () => ExpenseDoc); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
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

		FacilityPlace facilityPlace;

		[Display (Name = "Размещение на объекте")]
		public virtual FacilityPlace FacilityPlace {
			get { return facilityPlace; }
			set { SetField (ref facilityPlace, value, () => FacilityPlace); }
		}

		DateTime? autoWriteoffDate;

		[Display (Name = "День автосписания")]
		public virtual DateTime? AutoWriteoffDate {
			get { return autoWriteoffDate; }
			set { SetField (ref autoWriteoffDate, value, () => AutoWriteoffDate); }
		}

		private EmployeeIssueOperation employeeIssueOperation;

		[Display(Name = "Операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get { return employeeIssueOperation; }
			set { SetField(ref employeeIssueOperation, value); }
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

		//private NormItem normItem;

		//[Display(Name = "Строка нормы")]
		////В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		//public virtual NormItem NormItem
		//{
		//	get { return normItem; }
		//	set { SetField(ref normItem, value); }
		//}

		#endregion

		#region Расчетные свойства
		public virtual string Title {
			get { return String.Format ("Выдача со склада {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units.Name
			);}
		}
		#endregion

		public ExpenseItem ()
		{
		}

		#region Функции

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			if(expenseDoc.Operation == ExpenseOperations.Employee)
			{
				if (EmployeeIssueOperation == null)
					EmployeeIssueOperation = new EmployeeIssueOperation();
				EmployeeIssueOperation.Update(uow, this);
			}
			else if(EmployeeIssueOperation != null)
			{
				uow.Delete(EmployeeIssueOperation);
				EmployeeIssueOperation = null;
			}
		}

		#endregion

	}
}

