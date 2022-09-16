using System.ComponentModel.DataAnnotations;

namespace Workwear.Models.Import.Employees
{
	public enum DataTypeEmployee
	{
		[Display(Name = "Пропустить")]
		Unknown,
		[Display(Name = "Фамилия с инициалами")]
		NameWithInitials,
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
		[Display(Name = "Номер телефона")]
		Phone,
		[Display(Name = "Дата приёма на работу")]
		HireDate,
		[Display(Name = "Дата увольнения")]
		DismissDate,
		[Display(Name = "Дата рождения")]
		BirthDate,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Отдел")]
		Department,
		[Display(Name = "Должность")]
		Post,
		[Display(Name = "Размер/Рост")]
		SizeAndHeight
	}
}
