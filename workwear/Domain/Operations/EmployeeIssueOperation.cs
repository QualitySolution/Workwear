using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.Domain.Operations
{
	public class EmployeeIssueOperation : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		DateTime operationTime;

		public virtual DateTime OperationTime
		{
			get { return operationTime; }
			set { SetField(ref operationTime, value); }
		}

		private EmployeeCard employee;

		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee
		{
			get { return employee; }
			set { SetField(ref employee, value); }
		}

		private Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get { return nomenclature; }
			set { SetField(ref nomenclature, value); }
		}

		private decimal wearPercent;

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent
		{
			get { return wearPercent; }
			set { SetField(ref wearPercent, value); }
		}

		private int issued;

		[Display(Name = "Выдано")]
		public virtual int Issued
		{
			get { return issued; }
			set { SetField(ref issued, value); }
		}

		private int returned;

		[Display(Name = "Возвращено")]
		public virtual int Returned
		{
			get { return returned; }
			set { SetField(ref returned, value); }
		}

		private DateTime autoWriteoffDate;

		[Display(Name = "Дата износа")]
		public virtual DateTime AutoWriteoffDate
		{
			get { return autoWriteoffDate; }
			set { SetField(ref autoWriteoffDate, value); }
		}

		private EmployeeIssueOperation issuedOperation;

		[Display(Name = "Операция выдачи")]
		public virtual EmployeeIssueOperation IssuedOperation
		{
			get { return issuedOperation; }
			set { SetField(ref issuedOperation, value); }
		}

		private IncomeItem incomeOnStock;

		[Display(Name = "Строка поступление на склад")]
		public virtual IncomeItem IncomeOnStock
		{
			get { return incomeOnStock; }
			set { SetField(ref incomeOnStock, value); }
		}

		private NormItem normItem;

		[Display(Name = "Строка нормы")]
		public virtual NormItem NormItem
		{
			get { return normItem; }
			set { SetField(ref normItem, value); }
		}

		private string buhDocument;

		[Display(Name = "Документ бухгалтерского учета")]
		public virtual string BuhDocument
		{
			get { return buhDocument; }
			set { SetField(ref buhDocument, value); }
		}

		public EmployeeIssueOperation()
		{
		}
	}
}
