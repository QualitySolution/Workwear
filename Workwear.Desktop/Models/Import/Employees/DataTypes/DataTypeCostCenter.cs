using System;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeCostCenter : DataTypeEmployeeBase {
		private readonly DataParserEmployee dataParserEmployee;
		private readonly IImportModel model;

		public DataTypeCostCenter(DataParserEmployee dataParserEmployee, IImportModel model)
		{
			this.dataParserEmployee = dataParserEmployee ?? throw new ArgumentNullException(nameof(dataParserEmployee));
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			ColumnNameKeywords.AddRange(new [] {
				"мвз",
				"место возникновения затрат"
			});
			Data = DataTypeEmployee.CostCenter;
		}

		
		// Устанавливает сотруднику единственное МВЗ (доля 100%)
		public ChangeState SetEmployeeCostCenter(SheetRowEmployee row, string name, string code) {
			name = String.IsNullOrWhiteSpace(name) ? null : name.Trim();
			code = String.IsNullOrWhiteSpace(code) ? null : code.Trim();
			if(name == null && code == null)
				return new ChangeState(ChangeType.NotChanged);

			var employee = row.EditingEmployee;
			var current = employee.CostCenters;
			if(current.Count == 1 && current[0].Percent == 1 && IsSameCostCenter(current[0].CostCenter, name, code))
				return new ChangeState(ChangeType.NotChanged);

			var oldValue = current.Any() ? String.Join(", ", current.Select(x => x.CostCenter.Title)) : null;

			var costCenter = dataParserEmployee.UsedCostCenter.FirstOrDefault(x =>
				IsSameCostCenter(x, name, code));
			if(costCenter == null) {
				costCenter = new CostCenter {
					Name = name ?? "",
					Code = code
				};
				dataParserEmployee.UsedCostCenter.Add(costCenter);
			}

			employee.CostCenters.Clear();
			employee.CostCenters.Add(new EmployeeCostCenter(employee, costCenter, 1m));

			if(costCenter.Id == 0)
				return new ChangeState(ChangeType.NewEntity, oldValue: oldValue, willCreatedValues: new[] { costCenter.Title });
			return new ChangeState(ChangeType.ChangeValue, oldValue: oldValue, newValue: costCenter.Title);
		}

		private static bool IsSameCostCenter(CostCenter costCenter, string name, string code) => 
			(costCenter != null)
		 && (name == null || String.Equals(costCenter.Name, name, StringComparison.CurrentCultureIgnoreCase))
		 && (code == null || String.Equals(costCenter.Code, code, StringComparison.CurrentCultureIgnoreCase));
		
		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var name = row.CellStringValue(target);
			var codeColumn = model.GetColumnForDataType(DataTypeEmployee.CostCenterNumber);
			var code = codeColumn == null ? null : row.CellStringValue(codeColumn);
			row.AddColumnChange(target, SetEmployeeCostCenter(row, name, code));
		}
	}
}
