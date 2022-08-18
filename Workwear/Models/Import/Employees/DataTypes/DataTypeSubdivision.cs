using System;
using System.Linq;
using QS.DomainModel.UoW;
using workwear.Domain.Company;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypeSubdivision : DataTypeEmployeeBase {
		private readonly DataParserEmployee dataParserEmployee;

		public DataTypeSubdivision(DataParserEmployee dataParserEmployee)
		{
			this.dataParserEmployee = dataParserEmployee ?? throw new ArgumentNullException(nameof(dataParserEmployee));
			ColumnNameKeywords.Add("подразделение");
			Data = DataTypeEmployee.Subdivision;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowEmployee row, string value) {
			if(String.Equals(row.EditingEmployee.Subdivision?.Name, value, StringComparison.CurrentCultureIgnoreCase)) {
				return new ChangeState(ChangeType.NotChanged);
			}

			var subdivision = dataParserEmployee.UsedSubdivisions.FirstOrDefault(x =>
				String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase));
			if(subdivision == null) {
				subdivision = new Subdivision { Name = value };
				dataParserEmployee.UsedSubdivisions.Add(subdivision);
			}
			row.AddSetValueAction(ValueSetOrder, () => row.EditingEmployee.Subdivision = subdivision);
			if(subdivision.Id == 0)
				return new ChangeState(ChangeType.NewEntity, willCreatedValues: new[] { "Подразделение:" + subdivision.Name });
			return new ChangeState(ChangeType.ChangeValue, oldValue: row.EditingEmployee.Subdivision?.Name);
		}
		#endregion
	}
}
