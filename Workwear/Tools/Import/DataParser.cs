using System;
using System.Collections.Generic;

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
	}
}
