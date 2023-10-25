using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;

namespace Workwear.Domain.Company {
	
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы сотрудников",
		Nominative = "группа сотрудников",
		Genitive = "группы сотрудников"
	)]
	[HistoryTrace]
	public class EmployeeGroup: PropertyChangedBase, IDomainObject {
		
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

		public virtual IList<EmployeeGroupItem> AddItems(IList<EmployeeCard> employees) {
			IList<EmployeeGroupItem> itemsList = new List<EmployeeGroupItem>();
			foreach(var employee in employees) {
				var item = AddEmployee(employee);
				if (item != null) 
					itemsList.Add(item);
			}
			return itemsList;
		}

		public virtual EmployeeGroupItem AddEmployee(EmployeeCard employee) {
			if (Items.Any(i => i.Employee.Id == employee.Id))
				return null;
			
			EmployeeGroupItem item = new EmployeeGroupItem() {
				Employee = employee,
				Group = this
			};
			Items.Add(item);
			return item;
		}
	}
}
