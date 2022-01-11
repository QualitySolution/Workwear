using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Domain.Regulations
{
	public class NormCondition : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		public SexNormCondition SexNormCondition;
	}
	public enum SexNormCondition
	{
		[Display(Name = "Для всех")]
		ForAll,
		[Display(Name = "Только мужчинам")]
		OnlyMen,
		[Display(Name = "Только жешщинам")]
		OnlyWomen
	}
}