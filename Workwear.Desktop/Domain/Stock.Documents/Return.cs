﻿using System;
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
using Workwear.Repository.Operations;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы возврата",
		Nominative = "документ возврата",
		Genitive = "документа возврата"
		)]
	[HistoryTrace]
	public class Return : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		#region Свойства
		private Warehouse warehouse;
		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		private EmployeeCard employeeCard;
		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get => employeeCard;
			set { SetField (ref employeeCard, value, () => EmployeeCard); }
		}

		private IObservableList<ReturnItem> items = new ObservableList<ReturnItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<ReturnItem> Items {
			get => items;
			set { SetField (ref items, value, () => Items); }
		}
		#endregion
		public virtual string Title => $"Возврат от работника №{DocNumber ?? Id.ToString()} от {Date:d}";
		
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});
			
			if (DocNumber != null && DocNumber.Length > 15)
				yield return new ValidationResult ("Номер документа должен быть не более 15 символов", 
					new[] { this.GetPropertyName (o => o.DocNumber)});

			if(EmployeeCard == null)
				yield return new ValidationResult ("Сотрудник должен быть указан", 
					new[] { this.GetPropertyName (o => o.EmployeeCard)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});
		
			if(EmployeeCard != null)
				foreach (var item in items) {
					if(item.IssuedEmployeeOnOperation == null || !DomainHelper.EqualDomainObjects(item.IssuedEmployeeOnOperation.Employee, EmployeeCard))
						yield return new ValidationResult(
							$"{item.Nomenclature.Name}: номенклатура добавлена не из числящегося за данным сотрудником", 
							new[] { nameof(Items) });
				}

			foreach(var item in items) {
				if(item.Nomenclature == null)
					yield return new ValidationResult(
						$"Для \"{item.ItemName}\" необходимо выбрать складскую номенклатуру.",
						new[] { nameof(Items) });
				if(item.Amount > item.MaxAmount)
					yield return new ValidationResult(
						$" \"{item.ItemName}\" указано колличество больше выданного.",
						new[] { nameof(Items) });
			}
		}

		#endregion

		#region Строки документа
		public virtual ReturnItem AddItem(EmployeeIssueOperation issuedOperation, int maxCount = -1) {
			if(issuedOperation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.IssuedEmployeeOnOperation, issuedOperation))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return null;
			}
			var newItem = new ReturnItem(this) {
				//FIXME не учитываются другие операции (потенциальные списания)
				Amount = maxCount != -1 ? maxCount : issuedOperation.Issued,
				Nomenclature = issuedOperation.Nomenclature,
				WearSize = issuedOperation.WearSize,
				Height = issuedOperation.Height,
				IssuedEmployeeOnOperation = issuedOperation,
				Cost = issuedOperation.CalculateDepreciationCost(Date),
				WearPercent = issuedOperation.CalculatePercentWear(Date),
			};

			Items.Add(newItem);
			return newItem;
		}
		public virtual void RemoveItem(ReturnItem item) {
			Items.Remove (item);
		}
		#endregion

		public virtual void UpdateOperations(IUnitOfWork uow) {
			Items.ToList().ForEach(x => x.UpdateOperations(uow));
		}

		public virtual void UpdateEmployeeWearItems(IUnitOfWork uow) {
			EmployeeCard.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
			EmployeeCard.UpdateNextIssue(Items
				.Select(x => x.IssuedEmployeeOnOperation.ProtectionTools)
				.Where(x => x != null).Distinct().ToArray());
		}
	}
}

