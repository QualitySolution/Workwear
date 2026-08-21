using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Tools;

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

		public virtual string Title => $"Списание №{DocNumberText} от {Date:d}";

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { nameof(Date)});
			
			if (DocNumber != null && DocNumber.Length > 15)
				yield return new ValidationResult ("Номер документа должен быть не более 15 символов", 
					new[] {nameof(DocNumber)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { nameof(Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { nameof(Items)});

			var duplicateBarcodeTitles = Items
				.SelectMany(i => i.Barcodes)
				.Where(b => b != null)
				.GroupBy(b => b.Id != 0 ? (object)b.Id : b)
				.Where(g => g.Count() > 1)
				.Select(g => g.First().Title)
				.ToList();
			if(duplicateBarcodeTitles.Any())
				yield return new ValidationResult(
					$"В документе несколько раз указана одна и та же метка: {string.Join(", ", duplicateBarcodeTitles)}.",
					new[] { nameof(Items) });

			var baseParameters = (BaseParameters)validationContext.Items[nameof(BaseParameters)];
			foreach(var item in Items) {
				var barcodesCount = GetWriteoffBarcodesCount(item);
				if(barcodesCount > 0 && item.Amount != barcodesCount)
					yield return new ValidationResult(
						$" \"{item.Nomenclature.Name}\" количество должно быть равно количеству выбранных штрихкодов.",
						new[] { nameof(Items) });
				switch(item.WriteoffFrom) {
					case WriteoffFrom.Warehouse:
						if(baseParameters.CheckBalances && item.Amount > item.MaxAmount)
							yield return new ValidationResult(
								$" \"{item.Nomenclature.Name}\" указано колличество больше чем числится на складе.",
								new[] { nameof(Items) });
						break;
					case WriteoffFrom.Employee:
						if(item.Amount > item.MaxAmount)
							yield return new ValidationResult(
								$" \"{item.Nomenclature.Name}\" указано колличество больше чем числится за сотрудником.",
								new[] { nameof(Items) });
						break;
					case WriteoffFrom.DutyNorm:
						if(item.Amount>item.MaxAmount)
							yield return new ValidationResult(
								$" \"{item.Nomenclature.Name}\" указано колличество больше чем числится по дежурной норме.",
								new[] { nameof(Items) });
						break;
				}
			}
		}

		#endregion

		private int GetWriteoffBarcodesCount(WriteoffItem item) {
			switch(item.WriteoffFrom) {
				case WriteoffFrom.Employee:
					return item.EmployeeWriteoffOperation.BarcodeOperations.Count;
				case WriteoffFrom.DutyNorm:
					return item.DutyNormWriteOffOperation.BarcodeOperations.Count;
				case WriteoffFrom.Warehouse:
					return item.WarehouseBarcodeOperations.Count;
				default:
					return 0;
			}
		}

		public Writeoff ()
		{
		}

		#region Обработка строк
		public virtual WriteoffItem AddItem(EmployeeIssueOperation operation, int count, IEnumerable<Barcode> barcodes = null)
		{
			if(operation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			var barcodeList = barcodes?.ToList();
			if(barcodeList == null || !barcodeList.Any()) {
				var existing = Items.FirstOrDefault(p =>
					DomainHelper.EqualDomainObjects(p.EmployeeWriteoffOperation?.IssuedOperation, operation) && p.CanEditAmount);
				if(existing != null) {
					logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
					return existing;
				}
			}
			var item = new WriteoffItem(this, operation, count, barcodeList);
			Items.Add(item);
			return item;
		}

		public virtual WriteoffItem AddItem(DutyNormIssueOperation operation, int count, IEnumerable<Barcode> barcodes = null)
		{
			if(operation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			var barcodeList = barcodes?.ToList();
			if(barcodeList == null || !barcodeList.Any()) {
				var existing = Items.FirstOrDefault(p =>
					DomainHelper.EqualDomainObjects(p.DutyNormWriteOffOperation?.IssuedOperation, operation) && p.CanEditAmount);
				if(existing != null) {
					logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
					return existing;
				}
			}
			var item = new WriteoffItem(this, operation, count, barcodeList);
			Items.Add(item);
			return item;
		}

		public virtual WriteoffItem AddItem(StockPosition position, Warehouse warehouse, int count, IEnumerable<Barcode> barcodes = null)
		{
			if(position == null)
				throw new ArgumentNullException(nameof(position));

			if(warehouse == null)
				throw new ArgumentNullException(nameof(warehouse));

			var barcodeList = barcodes?.ToList();
			if(barcodeList == null || !barcodeList.Any()) {
				var existing = Items.FirstOrDefault(p =>
					p.WarehouseOperation?.ExpenseWarehouse == warehouse && position.Equals(p.StockPosition) && p.CanEditAmount);
				if(existing != null) {
					logger.Warn($"Позиция [{position}] для склада {warehouse.Name} уже добавлена. Пропускаем...");
					return existing;
				}
			}

			var item = new WriteoffItem(this, position, warehouse, count, barcodeList);
			Items.Add(item);
			return item;
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

