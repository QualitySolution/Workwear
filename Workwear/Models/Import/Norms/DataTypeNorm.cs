using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import.Norms
{
	public enum DataTypeNorm
	{
		[Display(Name = "Пропустить")]
		Unknown,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Должность")]
		Post,
		[Display(Name = "Номенклатура нормы")]
		ProtectionTools,
		[Display(Name = "Количество и период")]
		//Здесь порядок важен, он влияет на сохранение... Если есть обе колонки сначала записываем общую, а потом частные.
		PeriodAndCount,
		[Display(Name = "Количество")]
		Amount,
		[Display(Name = "Период")]
		Period,
	}
}
