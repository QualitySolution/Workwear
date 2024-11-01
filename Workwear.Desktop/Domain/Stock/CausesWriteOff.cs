using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.Stock {
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		NominativePlural = "причины списания",
		Nominative = "причина списания",
		Genitive = "причины списания")]
	[HistoryTrace]
	
	public class CausesWriteOff: PropertyChangedBase, IDomainObject {
		
		#region Свойства

		public virtual int Id { get; set; }
		
		private string name;

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название причины должно быть заполнено.")]
		[StringLength(120)]

		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}
		
		#endregion
	}
}
