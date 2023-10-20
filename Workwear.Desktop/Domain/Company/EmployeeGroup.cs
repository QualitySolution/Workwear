using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
		
		#region Генерируемые Свойства

		public virtual int Count => Items?.Count ?? 0;
		#endregion
		
		#region Хранимые Свойства

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
			get => String.IsNullOrWhiteSpace(comment) ? null : comment;  //Чтобы в базе хранить null, а не пустую строку. 
			set => SetField(ref comment, value);
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

		public virtual IList<EmployeeGroupItem> Add(IList<EmployeeCard> employees) {
			IList<EmployeeGroupItem> itemsLList = new List<EmployeeGroupItem>();
			foreach(var employee in employees)
				if(!Items.Any(x => x.Employee.Id == employee.Id))
					itemsLList.Add(Add(employee));
			return itemsLList;
		}

		public virtual EmployeeGroupItem Add(EmployeeCard employee) {
			if (Items.Any(i => i.Employee.Id == employee.Id))
				return null;
			EmployeeGroupItem item = new EmployeeGroupItem() {Employee = employee, Group = this};
			Items.Add(item);
			return item;
		}
	}
}
