using System;
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
			set {
				SetField(ref profession, value);
				if(Profession != null && String.IsNullOrWhiteSpace(Name))
					Name = Profession.Name;
			}
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

		/// <summary>
		/// Возвращает копию текущего объекта
		/// </summary>
		/// <returns>Копия текущего объекта Post.</returns>
		public virtual Post CopyPost()
		{
			Post newPost = new Post();
			newPost.Name = this.Name;
			newPost.Subdivision = this.subdivision;
			newPost.department = this.department;
			newPost.profession = this.profession;
			newPost.comments = this.comments;
			return newPost;
		}
	}
}

