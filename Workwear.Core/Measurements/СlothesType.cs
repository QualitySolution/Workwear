using System.ComponentModel.DataAnnotations;

namespace Workwear.Measurements
{
	public enum ClothesSex {
		[Display(Name = "Женская")]
		Women,
		[Display(Name = "Мужская")]
		Men,
		[Display(Name = "Универсальная")]
		Universal,
	}

	public class ClothesSexType : NHibernate.Type.EnumStringType
	{
		public ClothesSexType() : base(typeof(ClothesSex)) { }
	}

}

