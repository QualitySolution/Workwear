using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Operations 
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "оперерации на выдачу подменного фонда",
		Nominative = "оперерация на выдачу подменного фонда"
	)]
	[HistoryTrace]
	public class SubstituteFundOperation : PropertyChangedBase, IDomainObject 
	{
		#region Properties
		public virtual int Id { get; set; }
		
		DateTime operationTime = DateTime.Now;
		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime 
		{
			get => operationTime;
			set => SetField(ref operationTime, value);
		}
		
		private Barcode substituteBarcode;
		[Display(Name = "Штрихкод подменной номенкалтуры")]
		public virtual Barcode SubstituteBarcode 
		{
			get => substituteBarcode;
			set => SetField(ref substituteBarcode, value);
		}
		
		private EmployeeIssueOperation employeeIssueOperation;
		[Display(Name = "Операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get => employeeIssueOperation;
			set => SetField(ref employeeIssueOperation, value);
		}
		
		private WarehouseOperation warehouseOperation;
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation 
		{
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}

		private SubstituteFundOperation writeOffSubstituteFundOperation;
		[Display(Name = "Операция возврата подменной вещи")]
		public virtual SubstituteFundOperation WriteOffSubstituteFundOperation 
		{
			get => writeOffSubstituteFundOperation;
			set => SetField(ref writeOffSubstituteFundOperation, value);
		}
		#endregion
	}
}
