using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using NHibernate.Engine;

namespace Workwear.Domain.Company
{
	public enum Sex
	{
		[Display(Name = "Нет")]
		None,
		[Display(Name = "Мужской")]
		M,
		[Display(Name = "Женский")]
		F
	}
	
	public class SexStringType : NHibernate.Type.EnumStringType
	{
		public SexStringType() : base(typeof(Sex))
		{
		}

		public override void NullSafeSet(DbCommand st, object value, int index, bool[] settable, ISessionImplementor session)
		{
			if(Equals(value, Sex.None))
				value = null;
			base.NullSafeSet(st, value, index, settable, session);
		}
	}
}

