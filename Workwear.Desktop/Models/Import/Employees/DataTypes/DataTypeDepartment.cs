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
			if(String.IsNullOrEmpty(value) || IsSameDepartment(row.EditingEmployee.Department, value, row.EditingEmployee.Subdivision)) {
				return new ChangeState(ChangeType.NotChanged);
			}

			var department = dataParserEmployee.UsedDepartment.FirstOrDefault(x => IsSameDepartment(x, value, row.EditingEmployee.Subdivision));
			if(department == null) {
				department = new Department {
					Name = value,
					Subdivision = row.EditingEmployee.Subdivision,
					Comments = "Создан при импорте сотрудников из файла " + model.FileName
				};
				dataParserEmployee.UsedDepartment.Add(department);
			}

			var oldValue = row.EditingEmployee.Department;
			row.EditingEmployee.Department = department;
			if(department.Id == 0)
				return new ChangeState(ChangeType.NewEntity, oldValue: FullTitle(oldValue), willCreatedValues: new[] { FullTitle(department) });
			return new ChangeState(ChangeType.ChangeValue, oldValue: FullTitle(oldValue), newValue: FullTitle(department));
		}
		
		private string FullTitle(Department department) {
			if(department == null)
				return null;
			var title = department.Name;
			if (department.Subdivision != null)
				title += "\nв подразделении: " + department.Subdivision.Name;
			return title;
		}
		
		private bool IsSameDepartment(Department department, string departmentName, Subdivision departmentSubdivision) {
			if(department == null)
				return false;

			return String.Equals(department.Name, departmentName, StringComparison.CurrentCultureIgnoreCase)
			       && (department.Subdivision == null && departmentSubdivision == null ||
			           DomainHelper.EqualDomainObjects(department.Subdivision, departmentSubdivision));
		}
		#endregion
	}
}
