using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import
{
	public enum DataTypeWorkwearItems
	{
		[Display(Name = "Пропустить")]
		Unknown,
		[Display(Name = "Табельный номер")]
		PersonnelNumber,
		[Display(Name = "Номеклатура нормы")]
		ProtectionTools,
		[Display(Name = "Номеклатура выдачи")]
		Nomenclature,
		//[Display(Name = "Количество и период")]
		//PeriodAndCount,
		[Display(Name = "Размер и рост")]
		SizeAndGrowth,
		[Display(Name = "Дата выдачи")]
		IssueDate,
		[Display(Name = "Количество")]
		Count,
	}
}
