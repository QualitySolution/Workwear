using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using Gamma.Utilities;
using workwear.Domain.Operations;
using QS.Dialog;
using System.Reflection;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Neuter,
	NominativePlural = "массовое списание",
	Nominative = "массовое списание",
	PrepositionalPlural = "массовое списание")]
	public class MassExpense : StockDocument, IValidatableObject
	{
		[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы выдачи",
		Nominative = "документ массовой выдачи")]

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private Warehouse warehouseFrom;

		[Display(Name = "Склад списания")]
		public virtual Warehouse WarehouseFrom {
			get { return warehouseFrom; }
			set { SetField(ref warehouseFrom, value, () => WarehouseFrom); }
		}

		private IList<MassExpenseEmployee> employees = new List<MassExpenseEmployee>();

		[Display(Name = "Сотрудники")]
		public virtual IList<MassExpenseEmployee> Employees {
			get { return employees; }
			set { SetField(ref employees, value); }
		}

		GenericObservableList<MassExpenseEmployee> observableEmployeeCard;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<MassExpenseEmployee> ObservableEmployeeCard {
			get {
				if(observableEmployeeCard == null)
					observableEmployeeCard = new GenericObservableList<MassExpenseEmployee>(Employees);
				return observableEmployeeCard;
			}
		}

		private IList<MassExpenseNomenclature> itemsNomenclature = new List<MassExpenseNomenclature>();

		[Display(Name = "Номенлатура документа")]
		public virtual IList<MassExpenseNomenclature> ItemsNomenclature {
			get { return itemsNomenclature; }
			set { SetField(ref itemsNomenclature, value, () => ItemsNomenclature); }
		}

		GenericObservableList<MassExpenseNomenclature> observableItemsNomenclature;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<MassExpenseNomenclature> ObservableItemsNomenclature {
			get {
				if(observableItemsNomenclature == null)
					observableItemsNomenclature = new GenericObservableList<MassExpenseNomenclature>(ItemsNomenclature);
				return observableItemsNomenclature;
			}
		}

		IList<MassExpenseOperation> massExpenseOperation = new List<MassExpenseOperation>();
		public virtual IList< MassExpenseOperation> MassExpenseOperation {
			get { return massExpenseOperation; }
			set { SetField(ref massExpenseOperation, value, () => MassExpenseOperation); }
		}


		GenericObservableList<MassExpenseOperation> observableOperations;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<MassExpenseOperation> ObservableOperations {
			get {
				if(observableOperations == null)
					observableOperations = new GenericObservableList<MassExpenseOperation>(MassExpenseOperation);
				return observableOperations;
			}
		}


		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Date < new DateTime(1990, 1, 1))
				yield return new ValidationResult("Дата должна быть указана",
					new[] { this.GetPropertyName(o => o.Date) });

			if(Employees.Count == 0)
				yield return new ValidationResult("Документ должен содержать хотя бы одного сотрудника.",
					new[] { this.GetPropertyName(o => o.ItemsNomenclature) });

			if(ItemsNomenclature.Count == 0)
				yield return new ValidationResult("Документ должен содержать хотя бы одну номенклатуру.",
					new[] { this.GetPropertyName(o => o.ItemsNomenclature) });

			if(ItemsNomenclature.Any(i => i.Amount <= 0))
				yield return new ValidationResult("Документ не должен содержать номенклатур с нулевым количеством.",
					new[] { this.GetPropertyName(o => o.ItemsNomenclature) });

			if(warehouseFrom == null)
				yield return new ValidationResult("Склад выдачи должен быть указан",
				new[] { this.GetPropertyName(o => o.WarehouseFrom) });

			if (Employees.Any(x => x.EmployeeCard.FirstName.Length < 2 || x.EmployeeCard.LastName.Length < 2))
				yield return new ValidationResult("Поля с именем и фамилией сотрудников должны быть заполнены.",
				new[] { this.GetPropertyName(o => o.ItemsNomenclature) });



		}

		#endregion

		public virtual string Title => $"Массовая выдача №{Id} от {Date:d}";

		#region Nomenclature

		IList<StockBalanceDTO> StockBalances;

		public virtual MassExpenseNomenclature AddItemNomenclature(Nomenclature nomenclature, IInteractiveMessage message, IUnitOfWork uow)
		{
			if (warehouseFrom == null) {
				message.ShowMessage(ImportanceLevel.Warning, "Выберете склад", "Предупреждение");
				logger.Warn("Склад не выбран");
				return null;
			}

			if(ItemsNomenclature.Any(p => DomainHelper.EqualDomainObjects(p.Nomenclature, nomenclature))) {
				message.ShowMessage(ImportanceLevel.Warning, "Такая номенклатура уже добавлена", "Предупреждение");
				logger.Warn("Номенклатура уже добавлена. Пропускаем...");
				return null;
			}

			var listNom = ItemsNomenclature.Select(x => x.Nomenclature).Where(x => x == nomenclature).Distinct().ToList();


			var stockRepo = new StockRepository();
			var stock = stockRepo.StockBalances(uow, warehouseFrom, listNom, DateTime.Now);

			if (stock.Count == 0) {
				message.ShowMessage(ImportanceLevel.Warning, $"Номенклатуры \"{nomenclature.Name}\" нет на складе \"{warehouseFrom.Name}\"", "Предупреждение");
				logger.Warn($"Номенклатуры {nomenclature} нет на складе {warehouseFrom.Name}");
				return null;
			}


			var newItemNomenclature = new MassExpenseNomenclature(this) {
				Amount = 1,
				Nomenclature = nomenclature
			};

			ObservableItemsNomenclature.Add(newItemNomenclature);
			return newItemNomenclature;
		}

		public virtual string ValidateNomenclature(IUnitOfWork uow)
		{
			var listNom = ItemsNomenclature.Select(x => x.Nomenclature).Distinct().ToList();
			var stockRepo = new StockRepository();
			var stock = stockRepo.StockBalances(uow, warehouseFrom, listNom, DateTime.Now);
			string DisplayMessage = "";
			foreach(var item in stock) {
				if(item.Amount < Employees.Count) {
					DisplayMessage += $"Не хватает номеклатуры: {item.Nomenclature.Name} ";
					logger.Warn($"Не хватает номеклатуры: {item.Nomenclature.Name}");
				}
			}
			return "text";
			//return DisplayMessage;
		}

		public virtual void RemoveItemNomenclature(MassExpenseNomenclature nom)
		{
			ObservableItemsNomenclature.Remove(nom);
		}
		#endregion

		#region Employee

		public virtual void AddEmployee(EmployeeCard emp, IInteractiveMessage message)
		{

			if(emp.Id != 0 ? employees.Any(p => DomainHelper.EqualDomainObjects(p, emp)) : false) {
				message.ShowMessage(ImportanceLevel.Warning, "Такой сотрудник уже добавлен", "Предупреждение");
				logger.Warn("Сотрудник уже добавлен. Пропускаем...");
				return;
			}

			var masEmp = new MassExpenseEmployee();
			masEmp.DocumentMassExpense = this;
			masEmp.Sex = emp.Sex;
			masEmp.EmployeeCard = emp;
			masEmp.GlovesSize = emp.GlovesSize;
			masEmp.GlovesSizeStd = emp.GlovesSizeStd;
			masEmp.WearSize = emp.WearSize;
			masEmp.WearSizeStd = emp.WearSizeStd;
			masEmp.WearGrowth = emp.WearGrowth;
			masEmp.HeaddressSize = emp.HeaddressSize;
			masEmp.HeaddressSizeStd = emp.HeaddressSizeStd;
			masEmp.WinterShoesSize = emp.WinterShoesSize;
			masEmp.WinterShoesSizeStd = emp.WinterShoesSizeStd;


			ObservableEmployeeCard.Add(masEmp);
		}

		public virtual void RemoveEmployee(MassExpenseEmployee emp)
		{
			ObservableEmployeeCard.Remove(emp);
		}
		#endregion

		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			uow.Save(this);
			var ListMassExOperationInProgress = MassExpenseOperation.ToList();

			foreach(var employee in Employees) {
				foreach(var nom in ItemsNomenclature) {
					var op = ListMassExOperationInProgress.FirstOrDefault(x =>x.EmployeeIssueOperation.Employee.Id == employee.Id && x.WarehouseOperationExpense.Nomenclature.Id == nom.Id);
					if(op == null) {

						op = new MassExpenseOperation();
						op.MassExpenseDoc = this;
						op.EmployeeIssueOperation = new EmployeeIssueOperation();
						op.WarehouseOperationExpense = new WarehouseOperation();

					}
					else
						ListMassExOperationInProgress.Remove(op);

					EmployeeIssueOperation employeeIssueOperation = op.EmployeeIssueOperation;
					employeeIssueOperation.Employee = employee.EmployeeCard;
					employeeIssueOperation.Nomenclature = nom.Nomenclature;
					employeeIssueOperation.Size = employee.WearSize;
					employeeIssueOperation.WearGrowth = employee.WearGrowth;
					employeeIssueOperation.Issued = nom.Amount;
					employeeIssueOperation.StartOfUse = DateTime.Now;

					WarehouseOperation warehouseOperation = op.WarehouseOperationExpense;
					warehouseOperation.Nomenclature = nom.Nomenclature;
					warehouseOperation.OperationTime = DateTime.Now;
					warehouseOperation.ExpenseWarehouse = this.WarehouseFrom;
					warehouseOperation.ReceiptWarehouse = null;
					warehouseOperation.Size = nom.Nomenclature.SizeStd;
					warehouseOperation.Growth = employee.WearGrowth;
					warehouseOperation.Amount = nom.Amount;
					warehouseOperation.OperationTime = DateTime.Now;

					employeeIssueOperation.WarehouseOperation = warehouseOperation;
					
					uow.Save(warehouseOperation);
					uow.Save(op);
				}
			}
		}
	}
}
