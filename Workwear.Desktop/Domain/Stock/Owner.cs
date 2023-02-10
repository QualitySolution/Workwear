using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.Stock 
{
	[Appellative (
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "собственники имущества",
		Nominative = "собственник имущества",
		Genitive = "собственника имущества")] 
	[HistoryTrace]
	public class Owner : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; }

		private string name;
		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название собственника должно быть заполнено.")]
		[StringLength(180)]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private string description;
		[Display (Name = "Описание")]
		public virtual string Description {
			get => description;
			set => SetField(ref description, value);
		}

		private int priority;
		[Display(Name = "Приоритет")]
		public virtual int Priority {
			get => priority;
			set => SetField(ref priority, value);
		}

		#endregion
	}
}
