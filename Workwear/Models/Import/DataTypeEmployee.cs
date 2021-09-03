using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import
{
	public enum DataTypeEmployee
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
		[Display(Name = "Пол")]
		Sex,
		[Display(Name = "Табельный номер")]
		PersonnelNumber,
		[Display(Name = "UID карты")]
		CardKey,
		[Display(Name = "Дата приёма на работу")]
		HireDate,
		[Display(Name = "Дата увольнения")]
		DismissDate,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Должность")]
		Post,
	}
}
