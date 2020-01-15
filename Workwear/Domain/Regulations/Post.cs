using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace workwear.Domain.Regulations
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "должности",
		Nominative = "должность")]
	public class Post : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[StringLength (180)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion


		public Post ()
		{
		}
	}
}

