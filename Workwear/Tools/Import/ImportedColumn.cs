using System;
using System.ComponentModel.DataAnnotations;

namespace workwear.Tools.Import
{
	public class ImportedColumn
	{
		public string Title;

		public DataType DataType;

		public ImportedColumn()
		{
		}
	}

	public enum DataType
	{
		[Display(Name = "Пропустить")]
		Unknown,
		[Display(Name = "ФИО")]
		Fio,
		[Display(Name = "Фамилия")]
		LastName,
		[Display(Name = "Имя")]
		FirstName,
		[Display(Name = "Отчество")]
		Patronymic,
		[Display(Name = "Табельный номер")]
		PersonnelNumber,
		[Display(Name = "UID карты")]
		CardKey
	}
}
