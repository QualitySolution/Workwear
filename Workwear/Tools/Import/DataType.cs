using System.ComponentModel.DataAnnotations;

namespace workwear.Tools.Import
{
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
		[Display(Name = "Пол")]
		Sex,
		[Display(Name = "Табельный номер")]
		PersonnelNumber,
		[Display(Name = "UID карты")]
		CardKey
	}
}
