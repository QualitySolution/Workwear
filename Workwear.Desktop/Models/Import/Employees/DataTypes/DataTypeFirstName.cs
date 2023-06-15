using System;
using Gamma.Utilities;
using Workwear.Domain.Company;
using Workwear.Models.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeFirstName : DataTypeSimpleString {
		private readonly PersonNames personNames;

		public DataTypeFirstName(PersonNames personNames) : base(
			DataTypeEmployee.FirstName, x => x.FirstName) {
			this.personNames = personNames;
			ColumnNameRegExp = @"^(имя|first(_| )name|name)";
		}

		protected override ChangeState GetChangeState(SheetRowEmployee row, string value, string original) {
			var state = base.GetChangeState(row, value, original);
			if(row.EditingEmployee.Sex == Sex.None && !String.IsNullOrWhiteSpace(value)) {
				var detectedSex = personNames.GetSexByName(value);
				if(detectedSex != Sex.None) {
					row.AddSetValueAction(ValueSetOrder, () => row.EditingEmployee.Sex = detectedSex);
					state.AddCreatedValues(detectedSex.GetEnumTitle());
				}
				else 
					personNames.WriteToFileNotFoundNames(value);
			}
			return state;
		}
	}
}
