using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.Company {
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "места возникновения затрат",
		Nominative = "место возникновения затрат",
		Genitive = "место возникновения затрат"
	)]
	[HistoryTrace]
	public class CostCenter: PropertyChangedBase, IDomainObject {
		public virtual int Id { get; set; }

		private string code;
		[Display(Name = "Код ВМЗ")]
		[StringLength(14)]
		public virtual string Code {
			get => code;
			set => SetField(ref code, value);
		}

		private string name;
		[Display (Name = "Название")]
		[StringLength(300)]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		public virtual string Name {
			get => name;
			set => SetField (ref name, value);
		}
	}
}
