using System.ComponentModel.DataAnnotations;

namespace Workwear.Models.Import.Issuance
{
	public enum DataTypeWorkwearItems
	{
		[Display(Name = "Пропустить")]
		Unknown,
		[Display(Name = "Табельный номер")]
		PersonnelNumber,
		[Display(Name = "Фамилия с инициалами")]
		NameWithInitials,
		[Display(Name = "ФИО")]
		Fio,
		[Display(Name = "Номенклатура нормы")]
		ProtectionTools,
		[Display(Name = "Номенклатура выдачи")]
		Nomenclature,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Должность")]
		Post,
		[Display(Name = "Размер")]
		Size,
		[Display(Name = "Рост")]
		Growth,
		[Display(Name = "Размер и рост")]
		SizeAndGrowth,
		[Display(Name = "Дата выдачи")]
		IssueDate,
		[Display(Name = "Дата списания")]
		WriteoffDate,
		[Display(Name = "Количество")]
		Count,
	}
}
