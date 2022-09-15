using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.Company
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "отделы",
		Nominative = "отдел",
		Genitive = "отдела"
		)]
	[HistoryTrace]
	public class Department : PropertyChangedBase, IDomainObject
	{
		public Department()
		{
		}

		#region Свойства

		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название должно быть заполнено")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private string comments;
		[Display(Name = "Комментарии")]
		public virtual string Comments {
			get => comments;
			set => SetField(ref comments, value);
		}

		#endregion
	}
}
