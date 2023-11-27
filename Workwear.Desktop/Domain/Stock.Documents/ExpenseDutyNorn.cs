using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.Domain.Stock.Documents {
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы выдачи по дежурным нормам",
		Nominative = "документ выдачи по дежурным нормам",
		Genitive = "документа выдачи по дежурным нормам"
	)]
	[HistoryTrace]

	public class ExpenseDutyNorn : StockDocument{
		
		#region Генерирумые Сввойства
		public virtual string Title => $"Выдачча по деж. норме №{Id} от {Date:d}";
		#endregion
		
		#region Хранимые Сввойства

		private EmployeeCard responsibleEmployee;
		[Display (Name = "Ответственый сотрудник")]
		public virtual EmployeeCard ResponsibleEmployee {
			get { return responsibleEmployee; }
			set { SetField (ref responsibleEmployee, value); }
		}
		
		private DutyNorm dutyNorm;
		[Display (Name = "Объект выдачи (норма)")]
		[Required(ErrorMessage = "Дежурная норма должна быть указана.")]
		public virtual DutyNorm  DutyNorm {
			get { return dutyNorm; }
			set { SetField(ref dutyNorm, value); }
		}
		
		private Warehouse warehouse;
		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value); }
		}
		
		private IObservableList<ExpenseDutyNornItem> items = new ObservableList<ExpenseDutyNornItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<ExpenseDutyNornItem> Items {
			get { return items; }
			set { SetField (ref items, value); }
		}

		#endregion
		

		#region Методы
		public virtual ExpenseDutyNornItem AddItem(StockPosition position, int amount = 1) {
			var newItem = new ExpenseDutyNornItem() {
				Document = this,
				Amount = amount,
				Nomenclature = position.Nomenclature,
				WearSize = position.WearSize,
				Height = position.Height,
			};

			Items.Add(newItem);
			return newItem;
		}
		
		public virtual void UpdateOperations(IUnitOfWork uow,IInteractiveQuestion askUser, string signCardUid = null) {
			Items.ToList().ForEach(x => x.UpdateOperation(uow));
		}
		
		public virtual void RemoveItem(ExpenseDutyNornItem item) {
			Items.Remove(item);
		}
		#endregion
	}
}
