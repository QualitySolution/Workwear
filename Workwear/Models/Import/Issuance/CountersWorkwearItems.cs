using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import.Issuance
{
	public enum CountersWorkwearItems
	{
		[Display(Name = "Затронуто сотрудников")]
		UsedEmployees,
		[Display(Name = "Проставлено размеров в сотрудника")]
		EmployeesSetSize,
		[Display(Name = "Добавлено норм в сотрудника")]
		EmployeesAddNorm,
		[Display(Name = "Операций выдачи")]
		NewOperations,
		[Display(Name = "Новых номенклатур")]
		NewNomenclatures,
		[Display(Name = "Не найден сотрудник")]
		EmployeeNotFound,
		[Display(Name = "Не найдена потребность")]
		WorkwearItemNotFound,
		[Display(Name = "Пропущено строк")]
		SkipRows
	}
}
