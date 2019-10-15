using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Domain.Company
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "виды отпусков",
		Nominative = "вид отпуска")]
	public class VacationType : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private bool excludeFromWearing;

		[Display(Name = "Исключать дни из носки")]
		public virtual bool ExcludeFromWearing {
			get { return excludeFromWearing; }
			set { SetField(ref excludeFromWearing, value); }
		}

		private string comments;

		[Display(Name = "Комментарий")]
		public virtual string Comments {
			get { return comments; }
			set { SetField(ref comments, value); }
		}

		#endregion

		public VacationType()
		{
		}
	}
}
