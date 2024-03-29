﻿using System;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypePersonalNumber : DataTypeSimpleString {
		private readonly SettingsMatchEmployeesViewModel settings;

		public DataTypePersonalNumber(SettingsMatchEmployeesViewModel settingsMatchEmployees) : base(
			DataTypeEmployee.PersonnelNumber, x => x.PersonnelNumber, new []{					
				"tn",
				"табельный",
				"таб. №",
				"таб."//Если такой вариант будет пересекаться с другими полями его можно удалить.
          }) {
			this.settings = settingsMatchEmployees ?? throw new ArgumentNullException(nameof(settingsMatchEmployees));
		}

		protected override ChangeState GetChangeState(SheetRowEmployee row, string value, string original) {
			var modifiedValue = (settings.ConvertPersonnelNumber ? EmployeeParse.ConvertPersonnelNumber(value) : value)?.Trim();
			return base.GetChangeState(row, modifiedValue, value);
		}
	}
}
