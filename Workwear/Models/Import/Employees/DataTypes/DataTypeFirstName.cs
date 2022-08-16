using System;
using Gamma.Utilities;
using Workwear.Domain.Company;
using workwear.Models.Company;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypeFirstName : DataTypeSimpleString {
		private readonly PersonNames personNames;

		public DataTypeFirstName(PersonNames personNames) : base(
			DataTypeEmployee.FirstName, x => x.FirstName, new []{					
				"FIRST_NAME",
				"имя",
				"FIRST NAME"
          }) {
			this.personNames = personNames;
		}

		protected override ChangeState GetChangeState(SheetRowEmployee row, string value, string original) {
			var state = base.GetChangeState(row, value, original);
			if(row.EditingEmployee.Sex == Sex.None && !String.IsNullOrWhiteSpace(value)) {
				var detectedSex = personNames.GetSexByName(value);
				if(detectedSex != Sex.None) {
					row.AddSetValueAction(ValueSetOrder, () => row.EditingEmployee.Sex = detectedSex);
					state.AddCreatedValues(detectedSex.GetEnumTitle());
				}
			}
			return state;
		}
	}
}
