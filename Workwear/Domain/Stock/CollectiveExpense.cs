﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Repository.Operations;
using workwear.Tools;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "коллективные выдачи",
		Nominative = "коллективная выдача")]
	public class CollectiveExpense : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		private Warehouse warehouse;

		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		private IList<CollectiveExpenseItem> items = new List<CollectiveExpenseItem>();

		[Display (Name = "Строки документа")]
		public virtual IList<CollectiveExpenseItem> Items {
			get => items;
			set => SetField(ref items, value);
		}

		System.Data.Bindings.Collections.Generic.GenericObservableList<CollectiveExpenseItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual System.Data.Bindings.Collections.Generic.GenericObservableList<CollectiveExpenseItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new System.Data.Bindings.Collections.Generic.GenericObservableList<CollectiveExpenseItem> (Items);
				return observableItems;
			}
		}

		private IssuanceSheet issuanceSheet;
		[Display(Name = "Связанная ведомость")]
		public virtual IssuanceSheet IssuanceSheet {
			get => issuanceSheet;
			set => SetField(ref issuanceSheet, value);
		}

		#endregion

		#region Расчетные
		public virtual string Title => $"Коллективная выдача №{Id} от {Date:d}";
		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});
					
			if(Items.All(i => i.Amount <= 0))
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку с количеством больше 0.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount > 0 && i.Nomenclature == null))
				yield return new ValidationResult ("Документ не должен содержать строки без выбранной номенклатуры и с указанным количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});
		}
		#endregion

		#region Добавление удаление строк

		public virtual void AddItems(EmployeeCard employee, BaseParameters baseParameters)
		{
			foreach(var item in employee.WorkwearItems) {
				if(item.ProtectionTools?.Type.IssueType != IssueType.Сollective)
					continue;
				AddItem(item, baseParameters);
			}
		}

		public virtual CollectiveExpenseItem AddItem(EmployeeCard employee, StockPosition position, int amount = 1)
		{
			var newItem = new CollectiveExpenseItem() {
				Document = this,
				Employee = employee,
				Amount = amount,
				Nomenclature = position.Nomenclature,
				Size = position.Size,
				WearGrowth = position.Growth,
				WearPercent = position.WearPercent
			};

			ObservableItems.Add(newItem);
			return newItem;
		}

		public virtual CollectiveExpenseItem AddItem(EmployeeCardItem employeeCardItem, BaseParameters baseParameters)
		{
			if(employeeCardItem == null)
				throw new ArgumentNullException(nameof(employeeCardItem));

			if(Items.Any(x => employeeCardItem.IsSame(x.EmployeeCardItem)))
				return null;

			CollectiveExpenseItem newItem;
			if(employeeCardItem.BestChoiceInStock.Any())
				newItem = AddItem(employeeCardItem.EmployeeCard, employeeCardItem.BestChoiceInStock.First().StockPosition);
			else { 
				newItem = new CollectiveExpenseItem() {
					Document = this,
					Employee = employeeCardItem.EmployeeCard
				};
				ObservableItems.Add(newItem);
			}
			
			newItem.EmployeeCardItem = employeeCardItem;
			newItem.ProtectionTools = employeeCardItem.ProtectionTools;
			newItem.Amount = newItem.Nomenclature != null ? employeeCardItem.CalculateRequiredIssue(baseParameters) : 0;

			return newItem;
		}

		public virtual void RemoveItem(CollectiveExpenseItem item)
		{
			ObservableItems.Remove (item);
		}

		public virtual void CleanupItems()
		{
			foreach(var item in Items.Where(x => x.Amount <= 0).ToList()) {
				RemoveItem(item);
			}
		}

		public virtual void ResortItems()
		{
			//Items = Items.OrderBy(x => x.Employee.FullName).ThenBy(x => x.ProtectionTools.Name).ToList();
			//observableItems = null;
			//OnPropertyChanged(nameof(ObservableItems));
		}

		#endregion

		#region Методы

		public virtual void UpdateOperations(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow, baseParameters, askUser));
		}

		public virtual void PrepareItems(IUnitOfWork uow, BaseParameters baseParameters)
		{
			var cardItems = Items.Select(x => x.Employee).Distinct().SelectMany(x => x.WorkwearItems);
			EmployeeCard.FillWearInStockInfo(uow, baseParameters, Warehouse, Date, cardItems);
			foreach(var docItem in Items) {
				docItem.EmployeeCardItem = docItem.Employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools.IsSame(docItem.ProtectionTools));
			}
		}

		public virtual void UpdateEmployeeWearItems(IProgressBarDisplayable progress)
		{
			var groups = Items.GroupBy(x => x.Employee);
			foreach(var employeeGroup in groups) {
				progress.Add(text: $"Обновляем потребности {employeeGroup.Key.ShortName}");
				employeeGroup.Key.UpdateNextIssue(employeeGroup.Select(x => x.ProtectionTools).ToArray());
				progress.Add();
				employeeGroup.Key.FillWearRecivedInfo(new EmployeeIssueRepository(UoW));
				UoW.Save(employeeGroup.Key);
			}
		}
		#endregion

		#region Ведомость
		public virtual void CreateIssuanceSheet()
		{
			if(IssuanceSheet != null)
				return;

			IssuanceSheet = new IssuanceSheet {
				CollectiveExpense = this
			 };
			UpdateIssuanceSheet();
		}

		public virtual void UpdateIssuanceSheet()
		{
			if(IssuanceSheet == null)
				return;

			IssuanceSheet.Date = Date;
			IssuanceSheet.Subdivision = Items.GroupBy(x => x.Employee.Subdivision)
											 .Where(x => x.Key != null)
				                             .OrderByDescending(x => x.Count())
											 .FirstOrDefault()?.Key;

			foreach(var item in Items.ToList()) {
				if(item.IssuanceSheetItem == null && item.Amount > 0) 
					item.IssuanceSheetItem = IssuanceSheet.AddItem(item);

				if(item.IssuanceSheetItem != null)
					item.IssuanceSheetItem.UpdateFromCollectiveExpense();

				if(item.IssuanceSheetItem != null && item.Amount == 0)
					IssuanceSheet.Items.Remove(item.IssuanceSheetItem);
			}
		}
		#endregion
	}
}

