using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
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
		
		private int? kitNumber;
		[Display(Name = "Номер комплекта")]
		public virtual int? KitNumber {
			get => kitNumber;
			set => SetField(ref kitNumber, value);
		}

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

		private OverNormOperation overNormOperation;
		[Display(Name = "Операция выдачи вне нормы")]
		public virtual OverNormOperation OverNormOperation 
		{
			get => overNormOperation;
			set => SetField(ref overNormOperation, value);
		}
		#endregion
		#region Расчетные
		public virtual string Title => $"Операция с меткой {Barcode.Title}";
		public virtual DateTime OperationDate => EmployeeIssueOperation?.OperationTime ??
		                                          OverNormOperation?.OperationTime ??
		                                          WarehouseOperation?.OperationTime ??
		                                          throw new Exception("Нет даты связанной операции");
		public virtual string OperationTitle {
			get {
				if(EmployeeIssueOperation != null)
					return EmployeeIssueOperation.Issued > 0 && EmployeeIssueOperation.Returned == 0
						? $"Выдача сотруднику: {EmployeeIssueOperation.Employee.ShortName}"
							: EmployeeIssueOperation.Returned > 0 && EmployeeIssueOperation.Issued == 0 ?
							//TODO реализовать списание
								$"Возврат от сотрудника: {EmployeeIssueOperation.Employee.ShortName}"
									: throw new NotSupportedException();
				if(DutyNormIssueOperation != null)
					return DutyNormIssueOperation.Issued > 0 && DutyNormIssueOperation.Returned == 0
						? $"Выдача по дежурной норме: {DutyNormIssueOperation.DutyNorm.Name}"
							: DutyNormIssueOperation.Returned > 0 && DutyNormIssueOperation.Issued == 0 ?
							//TODO реализовать списание
								$"Возврат с дежурной нормы: {DutyNormIssueOperation.DutyNorm.Name}"
									: throw new NotSupportedException();
				if(OverNormOperation != null)
					return OverNormOperation.WarehouseOperation?.ExpenseWarehouse != null 
						? $"{OverNormOperation.Type} выдача сотруднику: {OverNormOperation.Employee.ShortName}"
							: OverNormOperation.WarehouseOperation?.ReceiptWarehouse != null ?
								$"Возврат вне нормы от сотрудника: {OverNormOperation.Employee.ShortName}"
									: throw new NotSupportedException();
				if(WarehouseOperation != null)
					return "Маркировка на складе.";
				
				throw new NotSupportedException();
			}
		}

		public virtual EmployeeCard CurrentEmployee {
			get {
				if(WarehouseOperation?.ReceiptWarehouse != null)
					return null;
				if(EmployeeIssueOperation?.Issued > 0)
					return EmployeeIssueOperation.Employee;
				if(OverNormOperation != null)
					return OverNormOperation.Employee;
				return null;
			}
		}

		public virtual Warehouse CurrentWarehouse => WarehouseOperation?.ReceiptWarehouse;

		public virtual DutyNorm CurrentDutyNorm {
			get {
				if(WarehouseOperation?.ReceiptWarehouse != null)
					return null;
				if(DutyNormIssueOperation?.Issued > 0)
					return DutyNormIssueOperation.DutyNorm;
				return null;
			}
		}
		#endregion
	}
}
