using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeDepartment : DataTypeEmployeeBase {
		private readonly DataParserEmployee dataParserEmployee;
		private readonly IImportModel model;

		public DataTypeDepartment(DataParserEmployee dataParserEmployee, IImportModel model)
		{
			this.dataParserEmployee = dataParserEmployee ?? throw new ArgumentNullException(nameof(dataParserEmployee));
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			ColumnNameKeywords.AddRange(new [] {
				"отдел",
				"бригада",
				"бригады"
			});
			Data = DataTypeEmployee.Department;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowEmployee row, string value) {
			if(String.Equals(row.EditingEmployee.Department?.Name, value, StringComparison.CurrentCultureIgnoreCase)) {
				return new ChangeState(ChangeType.NotChanged);
			}
			var department = dataParserEmployee.UsedDepartment.FirstOrDefault(x =>
				String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase)
				&& (row.EditingEmployee.Subdivision == null && x.Subdivision == null || 
				    DomainHelper.EqualDomainObjects(x.Subdivision, row.EditingEmployee.Subdivision)));
			if(department == null) {
				department = new Department {
					Name = value,
					Subdivision = row.EditingEmployee.Subdivision,
					Comments = "Создан при импорте сотрудников из файла " + model.FileName
				};
				dataParserEmployee.UsedDepartment.Add(department);
			}
			row.EditingEmployee.Department = department;
			if(department.Id == 0)
				return new ChangeState(ChangeType.NewEntity, willCreatedValues: new[] { "Отдел:" + department.Name });
			return new ChangeState(ChangeType.ChangeValue, oldValue: row.EditingEmployee.Department?.Name);
		}
		#endregion
	}
}
