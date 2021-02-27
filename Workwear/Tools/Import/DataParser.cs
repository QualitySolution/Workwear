using System;
using System.Collections.Generic;
using QS.Utilities.Text;
using workwear.Domain.Company;

namespace workwear.Tools.Import
{
	public class DataParser
	{
		public Dictionary<string, DataType> ColumnNames = new Dictionary<string, DataType>();

		public DataParser()
		{
			AddColumnName(DataType.Fio,
				"ФИО",
				"Ф.И.О.",
				"Фамилия Имя Отчество"
				);
			AddColumnName(DataType.CardKey,
				"CARD_KEY",
				"card",
				"uid"
				);
			AddColumnName(DataType.FirstName,
				"FIRST_NAME",
				"имя",
				"FIRST NAME"
				);
			AddColumnName(DataType.LastName,
				"LAST_NAME",
				"фамилия",
				"LAST NAME"
				);
			AddColumnName(DataType.Patronymic,
				"SECOND_NAME",
				"SECOND NAME",
				"Patronymic",
				"Patronymic name",
				"Отчество"
				);
			AddColumnName(DataType.PersonnelNumber,
				"TN",
				"Табельный номер"
				);
		}

		private void AddColumnName(DataType type, params string[] names)
		{
			foreach(var name in names)
				ColumnNames.Add(name.ToLower(), type);
		}

		#region Обработка данных
		public bool IsDiff(EmployeeCard employee, DataType dataType, string value)
		{
			if(String.IsNullOrWhiteSpace(value))
				return false;

			switch(dataType) {
				case DataType.CardKey:
					return !String.Equals(employee.CardKey, value, StringComparison.InvariantCultureIgnoreCase);
				case DataType.PersonnelNumber:
					return !String.Equals(employee.PersonnelNumber, value, StringComparison.InvariantCultureIgnoreCase);
				case DataType.LastName:
					return !String.Equals(employee.LastName, value, StringComparison.CurrentCultureIgnoreCase);
				case DataType.FirstName:
					return !String.Equals(employee.FirstName, value, StringComparison.CurrentCultureIgnoreCase);
				case DataType.Patronymic:
					return !String.Equals(employee.Patronymic, value, StringComparison.CurrentCultureIgnoreCase);
				case DataType.Fio:
					value.SplitFullName(out string lastName, out string firstName, out string patronymic);
					bool lastDiff = !String.IsNullOrEmpty(lastName) && !String.Equals(employee.LastName, value, StringComparison.CurrentCultureIgnoreCase);
					bool firstDiff = !String.IsNullOrEmpty(firstName) && !String.Equals(employee.FirstName, value, StringComparison.CurrentCultureIgnoreCase);
					bool patronymicDiff = !String.IsNullOrEmpty(patronymic) && !String.Equals(employee.Patronymic, value, StringComparison.CurrentCultureIgnoreCase);
					return lastDiff || firstDiff || patronymicDiff;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion
	}
}
