using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Domain.Users;
using workwear.Repository.Operations;
using workwear.Repository.Stock;
using workwear.Tools;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "коллективные выдачи",
		Nominative = "коллективная выдача",
		Genitive ="коллективной выдачи"
		)]
	[HistoryTrace]
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
					new[] { nameof(Date)});
					
			if(Items.All(i => i.Amount <= 0))
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку с количеством больше 0.", 
					new[] { nameof(Items)});

			if(Items.Any (i => i.Amount > 0 && i.Nomenclature == null))
				yield return new ValidationResult ("Документ не должен содержать строки без выбранной номенклатуры и с указанным количеством.", 
					new[] { nameof(Items)});

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

		#region Добавление удаление строк

		public virtual void AddItems(EmployeeCard employee, BaseParameters baseParameters)
		{
			foreach(var item in employee.WorkwearItems) {
				if(item.ProtectionTools?.Type.IssueType != IssueType.Collective)
					continue;
				AddItem(item, baseParameters);
			}
		}

		public virtual CollectiveExpenseItem AddItem(EmployeeCardItem employeeCardItem, StockPosition position = null, int amount = 0) 
		{
			var newItem = new CollectiveExpenseItem() {
				Document = this,
				Employee = employeeCardItem.EmployeeCard,
				Amount = amount,
				EmployeeCardItem = employeeCardItem,
				ProtectionTools = employeeCardItem.ProtectionTools
			};
			if(position != null) {
				newItem.Nomenclature = position.Nomenclature;
				newItem.WearSize = position.WearSize;
				newItem.Height = position.Height;
			}

			ObservableItems.Add(newItem);
			return newItem;
		}

		public virtual CollectiveExpenseItem AddItem(EmployeeCardItem employeeCardItem, BaseParameters baseParameters)
		{
			if(employeeCardItem == null)
				throw new ArgumentNullException(nameof(employeeCardItem));

			if(Items.Any(x => employeeCardItem.IsSame(x.EmployeeCardItem)))
				return null;

			var needPositionAmount = employeeCardItem.CalculateRequiredIssue(baseParameters); //Количество которое нужно выдать
			if (!employeeCardItem.BestChoiceInStock.Any()) return AddItem(employeeCardItem);
			foreach(var position in employeeCardItem.BestChoiceInStock) {
				var expancePositionAmount = 
					Items.Where(item => item.Nomenclature == position.Nomenclature 
					                    && item.WearSize.Id == position.WearSize.Id 
					                    && item.Height.Id == position.Height.Id)
						.Aggregate(position.Amount, (current, item) => current - item.Amount);  //Есть на складе

				if(expancePositionAmount >= needPositionAmount && position.WearPercent == 0)
					return AddItem(employeeCardItem, position.StockPosition, needPositionAmount);
			}
			return AddItem(employeeCardItem);
		}

		public virtual void RemoveItem(CollectiveExpenseItem item)
		{
			ObservableItems.Remove (item);
			Items.Remove(item);
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
			EmployeeCard.FillWearInStockInfo(uow, Warehouse, Date, cardItems);
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
		public virtual void CreateIssuanceSheet(UserSettings userSettings)
		{
			if(IssuanceSheet != null)
				return;

			IssuanceSheet = new IssuanceSheet {
				CollectiveExpense = this,
				Organization = userSettings?.DefaultOrganization,
				HeadOfDivisionPerson = userSettings?.DefaultLeader,
				ResponsiblePerson = userSettings?.DefaultResponsiblePerson,
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

