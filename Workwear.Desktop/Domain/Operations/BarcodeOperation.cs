using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Operations {

	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции с маркировкой",
		Nominative = "операция с маркировкой"
	)]
	public class BarcodeOperation : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		private Barcode barcode;
		[Display(Name = "Метка(штрихкод)")]
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

		private DutyNormIssueOperation dutyNormIssueOperation;

		[Display(Name = "Операция выдачи по дежурной норме")]
		public virtual DutyNormIssueOperation DutyNormIssueOperation {
			get => dutyNormIssueOperation;
			set => SetField(ref dutyNormIssueOperation, value);
		}
		private WarehouseOperation warehouseOperation;
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}
		#endregion
		#region Расчетные
		public virtual string Title => $"Операция с меткой {Barcode.Title}";
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
