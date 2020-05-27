using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using workwear.Domain.Regulations;

namespace workwear.Domain.Company
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

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private Department department;
		[Display(Name = "Отдел")]
		public virtual Department Department {
			get => department;
			set => SetField(ref department, value);
		}

		private Profession profession;
		[Display(Name = "Профессия")]
		public virtual Profession Profession {
			get => profession;
			set => SetField(ref profession, value);
		}

		private string comments;
		[Display(Name = "Комментарии")]
		public virtual string Comments {
			get => comments;
			set => SetField(ref comments, value);
		}

		#endregion


		public Post ()
		{
		}
	}
}

