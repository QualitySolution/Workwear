using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;

namespace Workwear.Domain.Company {
	
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы",
		Nominative = "группа",
		Genitive = "группы"
	)]
	[HistoryTrace]
	public class EmployeeGroup: PropertyChangedBase, IDomainObject {
		
		#region Свойства

		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value); }
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value); }
		}
		#endregion
		
		#region Коллеции
		private IObservableList<EmployeeGroupItem> items = new ObservableList<EmployeeGroupItem>();
		[Display (Name = "Члены группы")]
		public virtual IObservableList<EmployeeGroupItem> Items {
			get => items;
			set => SetField (ref items, value);
		}
		#endregion
	}
}
