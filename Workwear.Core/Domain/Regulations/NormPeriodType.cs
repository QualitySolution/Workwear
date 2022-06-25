using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Regulations
{
	public enum NormPeriodType
	{
		[Display(Name = "Год")]
		Year,
		[Display(Name = "Месяц")]
		Month,
		[Display(Name = "Смена")]
		Shift,
		[Display(Name = "До износа")]
		Wearout,
		[Display(Name = "Дежурный")]
		Duty
	}
}

