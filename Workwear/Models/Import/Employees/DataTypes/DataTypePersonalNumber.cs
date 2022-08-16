using System;
using workwear.ViewModels.Import;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypePersonalNumber : DataTypeSimpleString {
		private readonly SettingsMatchEmployeesViewModel settings;

		public DataTypePersonalNumber(SettingsMatchEmployeesViewModel settingsMatchEmployees) : base(
			DataTypeEmployee.PersonnelNumber, x => x.PersonnelNumber, new []{					
				"TN",
				"Табельный",
				"Таб. №",
				"Таб."//Если такой вариант будет пересекаться с другими полями его можно удалить.
          }) {
			this.settings = settingsMatchEmployees ?? throw new ArgumentNullException(nameof(settingsMatchEmployees));
		}

		protected override ChangeState GetChangeState(SheetRowEmployee row, string value, string original) {
			var modifiedValue = (settings.ConvertPersonnelNumber ? EmployeeParse.ConvertPersonnelNumber(value) : value)?.Trim();
			return base.GetChangeState(row, modifiedValue, value);
		}
	}
}
