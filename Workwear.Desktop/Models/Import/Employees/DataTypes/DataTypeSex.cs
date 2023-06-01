using System;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeSex : DataTypeEmployeeBase {

		public DataTypeSex()
		{
			ColumnNameKeywords.AddRange(new []{				
				"sex",
				"gender"
			});
			ColumnNameRegExp = @"(?<=^|\s)пол(?=$|\s)";
			Data = DataTypeEmployee.Sex;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowEmployee row, string value) {
			if(String.IsNullOrWhiteSpace(value))
				return new ChangeState(ChangeType.NotChanged);

			Sex detectedSex = Sex.None;
			//Первая М английская, вторая русская.
			if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || 
			   value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase)) {
				detectedSex = Sex.M;
			}
			if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || 
			   value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase)) {
				detectedSex = Sex.F;
			}
			if(detectedSex == Sex.None)
				return new ChangeState(ChangeType.ParseError);

			if(row.EditingEmployee.Sex == detectedSex)
				return new ChangeState(ChangeType.NotChanged);

			row.AddSetValueAction(ValueSetOrder, () => row.EditingEmployee.Sex = detectedSex);
			
			if(row.EditingEmployee.Id == 0)
				return new ChangeState(ChangeType.NewEntity);
			else
				return new ChangeState(ChangeType.ChangeValue, oldValue: row.EditingEmployee.Sex.GetEnumTitle());
		}
		#endregion
	}
}
