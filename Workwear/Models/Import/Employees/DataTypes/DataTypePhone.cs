using System;
using QS.Utilities.Numeric;
using workwear.ViewModels.Import;

namespace workwear.Models.Import.Employees.DataTypes {
	public class DataTypePhone : DataTypeSimpleString {
		private readonly PhoneFormatter formatter;

		public DataTypePhone(PhoneFormatter formatter) : base(
			DataTypeEmployee.Phone, x => x.PhoneNumber, new []{					
				"Телефон",
				"Номер телефона"
          }) {
			this.formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
			ColumnNameRegExp = @"(?<=^|\s)тел(?=$|\s|\.)";
		}

		protected override ChangeState GetChangeState(SheetRowEmployee row, string value, string original) {
			var modifiedValue = formatter.FormatString(value);
			var state = base.GetChangeState(row, modifiedValue, value);
			if(!String.IsNullOrWhiteSpace(value) && modifiedValue.Length != formatter.MaxStringLength) {
				state.ChangeType = ChangeType.ParseError;
				state.Error = "Телефонный номер не полный или в неизвестном формате.";
			}

			return state;
		}
	}
}
