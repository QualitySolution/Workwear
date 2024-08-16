using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты списания",
		Nominative = "акт списания",
		Genitive = "акта списания"
		)]
	[HistoryTrace]
	public class Writeoff : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		private IObservableList<WriteoffItem> items = new ObservableList<WriteoffItem>();

		[Display (Name = "Строки документа")]
		public virtual IObservableList<WriteoffItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}
		
		private Leader director;
		[Display (Name = "Утверждающее лицо")]
		public virtual Leader Director {
			get { return director; }
			set { SetField (ref director, value, () => Director); }
		}
		
		private Leader chairman;
		[Display (Name = "Председатель комиссии")]
		public virtual Leader Chairman {
			get { return chairman; }
			set { SetField (ref chairman, value, () => Chairman); }
		}
		
		private Organization organization;
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value, () => Organization); }
		}

		private IObservableList<Leader> members = new ObservableList<Leader>();
		[Display(Name = "Члены комиссии")]
		public virtual IObservableList<Leader> Members {
			get { return members; }
			set { SetField(ref members, value, () => Members); }
		}
		#endregion

		#region Methods
		public virtual void RemoveMember(Leader member) {
			Members.Remove (member);
		}
		
		public virtual void AddMember(Leader member) {
			if(Members.Any(p => DomainHelper.EqualDomainObjects(p, member))) {
				logger.Warn("Этот член комиссии уже добавлен. Пропускаем...");
				return;
			}
			Members.Add(member);
		}
		#endregion

		public virtual string Title{
			get{ return String.Format ("Акт списания №{0} от {1:d}", DocNumberText, Date);}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});
			
			if (DocNumber != null && DocNumber.Length > 15)
				yield return new ValidationResult ("Номер документа должен быть не более 15 символов", 
					new[] {nameof(DocNumber)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});
			
		}

		#endregion


		public Writeoff ()
		{
		}

		#region Обработка строк
		public virtual WriteoffItem AddItem(EmployeeIssueOperation operation, int count)
		{
			if(operation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.EmployeeWriteoffOperation?.IssuedOperation, operation))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return null;
			}
			var item = new WriteoffItem(this, operation, count);
			Items.Add(item);
			return item;
		}

		public virtual void AddItem(StockPosition position, Warehouse warehouse, int count)
		{
			if(position == null)
				throw new ArgumentNullException(nameof(position));

			if(warehouse == null)
				throw new ArgumentNullException(nameof(warehouse));

			if(Items.Any(p => p.WarehouseOperation?.ExpenseWarehouse == warehouse && position.Equals(p.StockPosition))) {
				logger.Warn($"Позиция [{position}] для склада {warehouse.Name} уже добавлена. Пропускаем...");
				return;
			}

			Items.Add (new WriteoffItem(this, position, warehouse, count));
		}

		public virtual void RemoveItem(WriteoffItem item)
		{
			Items.Remove (item);
		}

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow));
		}

		#endregion
	}
}

