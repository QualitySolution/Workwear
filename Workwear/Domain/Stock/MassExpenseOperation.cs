using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative( Gender = GrammaticalGender.Feminine,
		Nominative = "проводка выдачи списком",
		NominativePlural = "проводки выдачи списком",
		Genitive = "продки выдачи списком"
		)]
	[HistoryTrace]
	public class MassExpenseOperation : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		WarehouseOperation warehouseOperationExpense;

		[Display(Name = "Складская операция")]
		public virtual WarehouseOperation WarehouseOperationExpense {
			get { return warehouseOperationExpense; }
			set { SetField(ref warehouseOperationExpense, value); }
		}

		EmployeeIssueOperation employeeIssueOperation;

		[Display(Name = "Операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation {
			get { return employeeIssueOperation; }
			set { SetField(ref employeeIssueOperation, value); }
		}

		MassExpense massExpenseDoc;

		[Display(Name = "Документ массовой выдачи")]
		public virtual MassExpense MassExpenseDoc {
			get { return massExpenseDoc; }
			set { SetField(ref massExpenseDoc, value); }
		}
		#endregion

		#region Рассчетные
		public virtual string Title => $"Проводка {EmployeeIssueOperation.Employee.ShortName} <- {EmployeeIssueOperation.Nomenclature.Name} x {employeeIssueOperation.Issued}";
		#endregion
	}
}
