using System;
using System.Collections.Generic;
using System.Linq;
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
			AddColumnName(DataType.Sex,
				"Sex",
				"Gender",
				"Пол"
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
				case DataType.Sex:
					//Первая М английская, вторая русская.
					if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase))
						return employee.Sex != Sex.M;
					if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase))
						return employee.Sex != Sex.F;
					return false;
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

		public EmployeeCard PrepareToSave(SheetRow row)
		{
			var employee = row.Employees.FirstOrDefault() ?? new EmployeeCard();
			//Здесь колонки сортируются чтобы процесс обработки данных был в порядке следования описания типов в Enum
			//Это надо для того чтобы наличие 2 полей с похожими данными заполнялись правильно. Например чтобы отдельное поле с фамилией могло перезаписать значение фамилии поученой из общего поля ФИО.
			foreach(var column in row.ChangedColumns.OrderBy(x => x.DataType)) {
				SetValue(employee, column.DataType, row.CellValue(column.Index));
			}
			return employee;
		}

		private void SetValue(EmployeeCard employee, DataType dataType, string value)
		{
			if(String.IsNullOrWhiteSpace(value))
				return;

			switch(dataType) {
				case DataType.CardKey:
					employee.CardKey = value;
					break;
				case DataType.PersonnelNumber:
					employee.PersonnelNumber = value;
					break;
				case DataType.LastName:
					employee.LastName = value;
					break;
				case DataType.FirstName:
					employee.FirstName = value;
					break;
				case DataType.Patronymic:
					employee.Patronymic = value;
					break;
				case DataType.Sex:
					//Первая М английская, вторая русская.
					if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase))
						employee.Sex = Sex.M;
					if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase))
						employee.Sex = Sex.F;
					break;
				case DataType.Fio:
					value.SplitFullName(out string lastName, out string firstName, out string patronymic);
					if(!String.IsNullOrEmpty(lastName) && !String.Equals(employee.LastName, value, StringComparison.CurrentCultureIgnoreCase))
						employee.LastName = lastName;
					if(!String.IsNullOrEmpty(firstName) && !String.Equals(employee.FirstName, value, StringComparison.CurrentCultureIgnoreCase))
						employee.FirstName = firstName;
					if(!String.IsNullOrEmpty(patronymic) && !String.Equals(employee.Patronymic, value, StringComparison.CurrentCultureIgnoreCase))
						employee.Patronymic = patronymic;
					break;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion
	}
}
