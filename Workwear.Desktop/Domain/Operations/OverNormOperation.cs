using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Company;

namespace Workwear.Domain.Operations {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции выдачи вне нормы",
		Nominative = "операция выдачи вне нормы"
	)]
	[HistoryTrace]
	public class OverNormOperation : PropertyChangedBase, IDomainObject 
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
		
		DateTime lastUpdate = DateTime.Now;
		[Display(Name = "Время последнего обновления операции")]
		public virtual DateTime LastUpdate 
		{
			get => lastUpdate;
			set => SetField(ref lastUpdate, value);
		}

		private OverNormType type;
		[Display(Name = "Тип операции выдачи вне нормы")]
		public virtual OverNormType Type 
		{
			get => type;
			set => SetField(ref type, value);
		}

		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee 
		{
			get => employee;
			set => SetField(ref employee, value);
		}
		
		private WarehouseOperation warehouseOperation;
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation 
		{
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}
		
		private EmployeeIssueOperation employeeIssueOperation;
		[Display(Name = "Операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get => employeeIssueOperation;
			set => SetField(ref employeeIssueOperation, value);
		}

		private OverNormOperation writeOffOverNormOperation;
		[Display(Name = "Операция возврата операции вдачи выдачи вне нормы")]
		public virtual OverNormOperation WriteOffOverNormOperation 
		{
			get => writeOffOverNormOperation;
			set => SetField(ref writeOffOverNormOperation, value);
		}
		
		private IList<BarcodeOperation> barcodeOperations = new List<BarcodeOperation>();
		[Display(Name = "Операции со штрихкодами")]
		public virtual IList<BarcodeOperation> BarcodeOperations 
		{
			get => barcodeOperations;
			set => SetField(ref barcodeOperations, value);
		}
		#endregion

		#region Not Mapped Propertis
		public virtual string Title => $"Операция выдачи выдачи вне нормы ({Type.GetAttribute<DisplayAttribute>().Name}) {WarehouseOperation.Nomenclature.Name} в количестве {WarehouseOperation.Amount}";
		#endregion
	}
	
	public enum OverNormType
	{
		[Display(Name = "Разовая")]
		Simple,
		[Display(Name = "Подменная")]
		Substitute,
		[Display(Name = "Гостевая")]
		Guest
	}
}
