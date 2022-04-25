﻿using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import
{
	public enum DataTypeWorkwearItems
	{
		[Display(Name = "Пропустить")]
		Unknown,
		[Display(Name = "Табельный номер")]
		PersonnelNumber,
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
		[Display(Name = "Количество")]
		Count,
	}
}
