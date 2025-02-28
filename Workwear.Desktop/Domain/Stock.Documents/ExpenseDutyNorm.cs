using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Statements;
using Workwear.Repository.Stock;
using Workwear.Tools;

namespace Workwear.Domain.Stock.Documents {
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы выдачи по дежурным нормам",
		PrepositionalPlural = "документах выдачи по дежурным нормам",
		Nominative = "документ выдачи по дежурной норме",
		Genitive = "документа выдачи по дежурной норме"
	)]
	[HistoryTrace]

	public class ExpenseDutyNorm : StockDocument, IValidatableObject{
		
		#region Генерирумые Сввойства
		public virtual string Title => $"Выдача по деж. норме №{DocNumberText} от {Date:d}";
		#endregion
		
		#region Хранимые Сввойства

		private EmployeeCard responsibleEmployee;
		[Display (Name = "Ответственный сотрудник")]
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
		
		private IObservableList<ExpenseDutyNormItem> items = new ObservableList<ExpenseDutyNormItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<ExpenseDutyNormItem> Items {
			get { return items; }
			set { SetField (ref items, value); }
		}
		
		private IssuanceSheet issuanceSheet;
		[Display(Name = "Связанная ведомость")]
		public virtual IssuanceSheet IssuanceSheet {
			get => issuanceSheet;
			set => SetField(ref issuanceSheet, value);
		}
		

		#endregion
		
		#region Методы
		public virtual ExpenseDutyNormItem AddItem(StockPosition position, int amount = 1, DutyNormItem dutyNormItem = null) {
			if(position == null)
				return null;
			var newItem = new ExpenseDutyNormItem() {
				Document = this,
				Amount = amount,
				Nomenclature = position.Nomenclature,
				WearSize = position.WearSize,
				Height = position.Height,
				WearPercent = position.WearPercent
			};

			newItem.DutyNormItem = dutyNormItem ?? DutyNorm.GetItem(position.Nomenclature);
			newItem.ProtectionTools = newItem.DutyNormItem?.ProtectionTools;
			Items.Add(newItem);
			return newItem;
		}
		
		public virtual ExpenseDutyNormItem AddItem(ProtectionTools protectionTools, int amount = 0) {
			if(protectionTools == null)
				return null;
			var newItem = new ExpenseDutyNormItem() {
				Document = this,
				ProtectionTools = protectionTools,
				Amount = amount,
			};
			newItem.DutyNormItem = DutyNorm.GetItem(protectionTools);
			Items.Add(newItem);
			return newItem;
		}
		
		public virtual void UpdateOperations(IUnitOfWork uow,IInteractiveQuestion askUser, string signCardUid = null) {
			Items.ToList().ForEach(x => x.UpdateOperation(uow));
		}
		
		public virtual void RemoveItem(ExpenseDutyNormItem item) {
			Items.Remove(item);
		}
		
		#region Ведомость
		public virtual void CreateIssuanceSheet(Organization defaultOrganization, Leader defaultLeader, Leader defaultResponsiblePerson)
		{
			if(IssuanceSheet != null)
				return;

			IssuanceSheet = new IssuanceSheet {
				ExpenseDutyNorm = this,
				Organization = defaultOrganization,
				HeadOfDivisionPerson = defaultLeader,
				ResponsiblePerson = defaultResponsiblePerson,
			};
			UpdateIssuanceSheet();
		}
		public virtual void UpdateIssuanceSheet()
		{
			if(IssuanceSheet == null)
				return;
			
			if(ResponsibleEmployee==null)
				throw new NullReferenceException("Для обновления ведомости ответственный сотрудник должен быть указан.");

			IssuanceSheet.Date = Date;
			if(ResponsibleEmployee.Subdivision != null)
				IssuanceSheet.Subdivision = ResponsibleEmployee.Subdivision;

			foreach(var item in Items.ToList()) {
				if(item.IssuanceSheetItem == null && item.Amount > 0) 
					item.IssuanceSheetItem = IssuanceSheet.AddItem(item);

				if(item.IssuanceSheetItem != null)
					item.IssuanceSheetItem.UpdateFromExpenseDuty();
			}
		}
		
		#endregion
		#endregion
		
		#region IValidatable
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { nameof(Date)});
			if (DocNumber != null && DocNumber.Length > 15)
				yield return new ValidationResult ("Номер документа должен быть не более 15 символов", 
					new[] { nameof(DocNumber)});
			if(DutyNorm == null)
				yield return new ValidationResult ("Норма должна быть указана", 
					new[] { nameof (DutyNorm)});
			if(Warehouse == null)
				yield return new ValidationResult ("Склад должен быть указан", 
					new[] { nameof (Warehouse)});
			if(Items.All(i => i.Amount <= 0))
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку с количеством больше 0.", 
					new[] { nameof (Items)});
			if(Items.Any (i => i.ProtectionTools == null))
				yield return new ValidationResult ("Документ не должен содержать строки с неуказанной потребностью (Номенклатурой нормы).", 
					new[] { nameof (Items)});
			
			//Проверка наличия на складе
			var baseParameters = (BaseParameters)validationContext.Items[nameof(BaseParameters)];
			if(UoW != null && baseParameters.CheckBalances) {
				var repository = new StockRepository();
				var nomenclatures = Items.Where(x => x.Nomenclature != null).Select(x => x.Nomenclature).Distinct().ToList();
				var excludeOperations = Items.Where(x => x.WarehouseOperation?.Id > 0).Select(x => x.WarehouseOperation).ToList();
				var balance = repository.StockBalances(UoW, Warehouse, nomenclatures, Date, excludeOperations);

				var positionGroups = Items.Where(x => x.Nomenclature != null).GroupBy(x => x.StockPosition);
				foreach(var position in positionGroups) {
					var amount = position.Sum(x => x.Amount);
					if(amount == 0)
						continue;

					var stockExist = balance.FirstOrDefault(x => x.StockPosition.Equals(position.Key));

					if(stockExist == null) {
						yield return new ValidationResult($"На складе отсутствует - {position.Key.Title}", new[] { nameof(Items) });
						continue;
					}

					if(stockExist.Amount < amount) {
						yield return new ValidationResult($"Недостаточное количество - {position.Key.Title}, Необходимо: {amount} На складе: {stockExist.Amount}", new[] { nameof(Items) });
						continue;
					}
				}
			}
		}
		#endregion
	}
}
