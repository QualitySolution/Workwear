using System;
using System.Collections.Generic;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeCardNumber : DataTypeSimpleString {
		private readonly HashSet<string> existCardNumbers;
		private readonly CountersViewModel counters;

		public DataTypeCardNumber(HashSet<string> existCardNumbers, CountersViewModel counters) : base(DataTypeEmployee.CardNumber, x => x.CardNumber, new []{
			"CardNumber",
			"Номер карточки"
		}) {
			this.existCardNumbers = existCardNumbers ?? throw new ArgumentNullException(nameof(existCardNumbers));
			this.counters = counters ?? throw new ArgumentNullException(nameof(counters));
		}

		protected override ChangeState GetChangeState(SheetRowEmployee row, string value, string originalValue) {
			if(!String.IsNullOrWhiteSpace(value)) {
				if(existCardNumbers.Contains(value)) {
					counters.AddCount(CountersEmployee.DuplicateCardNumbers);
					return new ChangeState(ChangeType.Duplicate, error: "Карточка с таким номером уже существует.");
				}
				existCardNumbers.Add(value);
			}

			return base.GetChangeState(row, value, originalValue);
		}
	}
}
