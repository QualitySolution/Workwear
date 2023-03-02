using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Operations {

	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции со штрихкодами",
		Nominative = "операция со штрихкодом"
	)]
	public class BarcodeOperation : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		private Barcode barcode;
		[Display(Name = "Штрихкод")]
		public virtual Barcode Barcode {
			get => barcode;
			set => SetField(ref barcode, value);
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
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}
		#endregion
		#region Расчетные
		public virtual string Title => $"Операция со штрихкодом {Barcode.Title}";
		public virtual DateTime? OperationDate => EmployeeIssueOperation?.OperationTime ?? WarehouseOperation?.OperationTime;
		public virtual string OperationTitle {
			get {
				if(EmployeeIssueOperation?.Issued > 0)
					return $"Выдача сотруднику: {EmployeeIssueOperation.Employee.ShortName}";
				return "???";
			}
		}
		#endregion
	}
}
