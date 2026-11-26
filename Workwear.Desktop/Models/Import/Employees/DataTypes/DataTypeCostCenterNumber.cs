using System;
using QS.DomainModel.UoW;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeCostCenterNumber : DataTypeEmployeeBase {
		private readonly DataParserEmployee dataParserEmployee;
		private readonly DataTypeCostCenter dataTypeCostCenter;
		private readonly IImportModel model;

		public DataTypeCostCenterNumber(DataTypeCostCenter dataTypeCostCenter, IImportModel model)
		{
			this.dataTypeCostCenter = dataTypeCostCenter ?? throw new ArgumentNullException(nameof(dataTypeCostCenter));
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			ColumnNameKeywords.Add("код мвз");
			ColumnNameDetectPriority = 1;
			Data = DataTypeEmployee.CostCenterNumber;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var code = row.CellStringValue(target);
			var nameColumn = model.GetColumnForDataType(DataTypeEmployee.CostCenter);
			var name = nameColumn == null ? null : row.CellStringValue(nameColumn);
			row.AddColumnChange(target, dataTypeCostCenter.SetEmployeeCostCenter(row, name, code));
		}
	}
}
