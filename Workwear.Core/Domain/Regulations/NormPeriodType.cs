using System.ComponentModel.DataAnnotations;
#if !NETSTANDARD
using NHibernate.Engine;
#endif

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
	}
#if !NETSTANDARD
	public class NormPeriodTypeType : NHibernate.Type.EnumStringType
	{
		public NormPeriodTypeType() : base(typeof(NormPeriodType))
		{
		}
	}
#endif
}

