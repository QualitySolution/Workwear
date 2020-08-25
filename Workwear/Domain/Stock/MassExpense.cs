using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Statements;
using workwear.Measurements;
using workwear.Repository.Stock;

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

		string DisplayMessage { get; set; } = "";
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
		private IssuanceSheet issuanceSheet;
		[Display(Name = "Связанная ведомость")]
		public virtual IssuanceSheet IssuanceSheet {
			get => issuanceSheet;
			set => SetField(ref issuanceSheet, value);
		}

		public virtual void CreateIssuanceSheet()
		{
			if(IssuanceSheet != null)
				return;

			IssuanceSheet = new IssuanceSheet {
				MassExpense = this
			};
			UpdateIssuanceSheet();
		}

		public virtual void UpdateIssuanceSheet()
		{
			if(IssuanceSheet == null)
				return;

			if(Employees.Count < 1)
				throw new NullReferenceException("Для обновления ведомости сотрудники должны быть указаны.");
			if(itemsNomenclature.Count < 1 )
				throw new NullReferenceException("Для обновления ведомости номенклатура должна быть указана.");

			IssuanceSheet.Date = Date;

			foreach(var emp in Employees) {
				foreach(var nomen in ItemsNomenclature) {
					var warehouseOperation = massExpenseOperation.FirstOrDefault(x => x.EmployeeIssueOperation.Employee == emp.EmployeeCard && x.EmployeeIssueOperation.Nomenclature == nomen.Nomenclature);
				nomen.IssuanceSheetItem = IssuanceSheet.AddItem(emp, nomen, warehouseOperation.EmployeeIssueOperation);
				}
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
					new[] { this.GetPropertyName(o => o.Employees) });

			if(ItemsNomenclature.Count == 0)
				yield return new ValidationResult("Документ должен содержать хотя бы одну номенклатуру.",
					new[] { this.GetPropertyName(o => o.ItemsNomenclature) });

			if(ItemsNomenclature.Any(i => i.Amount <= 0))
				yield return new ValidationResult("Документ не должен содержать номенклатур с нулевым количеством.",
					new[] { this.GetPropertyName(o => o.ItemsNomenclature) });

			if(warehouseFrom == null)
				yield return new ValidationResult("Склад выдачи должен быть указан",
				new[] { this.GetPropertyName(o => o.WarehouseFrom) });

			if (Employees.Any(x => x.EmployeeCard.FirstName.Length < 2 || x.EmployeeCard.LastName.Length < 2 
				|| x.Sex == Sex.None))
				yield return new ValidationResult("Поля с именем, фамилией и полом сотрудников должны быть заполнены.",
				new[] { this.GetPropertyName(o => o.ItemsNomenclature) });

			if (DisplayMessage.Length > 0)
				yield return new ValidationResult("Не для всех сотрудников возможно подобрать номенклатуру",
				new[] { this.GetPropertyName(o => o.DisplayMessage) });

		}

		#endregion

		public virtual string Title => $"Массовая выдача №{Id} от {Date:d}";

		#region Nomenclature

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

			List<Nomenclature> listNom = new List<Nomenclature>();
			listNom.Add(nomenclature);

			var stockRepo = new StockRepository();
			var stock = stockRepo.StockBalances(uow, warehouseFrom, listNom, this.Date);

			if (stock.Count == 0) {
				message.ShowMessage(ImportanceLevel.Warning, $"Номенклатуры «{nomenclature.Name}» нет на складе \"{warehouseFrom.Name}\"", "Предупреждение");
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
			var stockRepo = new StockRepository();
			List<Nomenclature> nomenclatures = new List<Nomenclature>();
			foreach(var nom in ItemsNomenclature)
				nomenclatures.Add(nom.Nomenclature);
			var stock = stockRepo.StockBalances(uow, warehouseFrom, nomenclatures, this.Date);
			if(massExpenseOperation!= null)
				stock = addOldNomenklInStockBalance(stock);
			foreach(var emp in employees)
				emp.ListWarehouseOperation.Clear();

			DisplayMessage = "";
			foreach (var nom in ItemsNomenclature) {
				if (nom.Nomenclature.Type.Category == ItemTypeCategory.property || nom.Nomenclature.Type.WearCategory == СlothesType.PPE) {
					List<Nomenclature> newListNomenPPE = new List<Nomenclature>();
					newListNomenPPE.Add(nom.Nomenclature);
					var stock2 = stockRepo.StockBalances(uow, warehouseFrom, newListNomenPPE, this.Date);
					if((stock2.Count < 1 ? 0 : stock2[0].Amount) < employees.Count * nom.Amount)
						DisplayMessage += $"Номенклатуры «{nom.Nomenclature.Name}» на складе недостаточно({stock2[0].Amount}). \n";
					continue;
				}

				foreach(var emp in employees) {
					var sizeAndStd = emp.GetSize(nom.Nomenclature.Type.WearCategory);
					if (string.IsNullOrEmpty(sizeAndStd.Size)) {
						if (!DisplayMessage.Contains("Не для всех сотрудников"))
							DisplayMessage += $"Не для всех сотрудников указан размер одежды! \n";
						continue;
					}
					var EnableSize = SizeHelper.MatchSize(sizeAndStd.StandardCode, sizeAndStd.Size, SizeUsePlace.Сlothes);
					var stockBalanse = stock.Where(x => EnableSize.Any(y => x.Size == y.Size && 
					(y.StandardCode == x.Nomenclature.SizeStd || (x.Nomenclature.SizeStd != null && x.Nomenclature.SizeStd.ToString() == "UnisexWearRus")))).ToList();
					int allCount = 0;
					int needCount = nom.Amount;
					foreach(var item in stockBalanse)
						allCount += item.Amount;
					if(allCount < nom.Amount)
						DisplayMessage += $"Номенклатуры «{nom.Nomenclature.Name}» размера {sizeAndStd.Size} на складе недостаточно \n";
					else {
						var warOper = new WarehouseOperation();
						foreach(var item in stockBalanse) {
							if(item.Amount < 1 || needCount == 0) continue;
							item.Amount--;
							needCount--;
							warOper.Amount++;
							warOper.Nomenclature = item.Nomenclature;
							warOper.Size = item.Size;
							warOper.Growth = item.Growth;

						}
						emp.ListWarehouseOperation.Add(warOper);
					}
				}
			}

			return DisplayMessage;

		}

		private IList<StockBalanceDTO> addOldNomenklInStockBalance(IList<StockBalanceDTO> stockBalances)
		{
			foreach(var stBalance in stockBalances) {
				var sumOldAmount = massExpenseOperation.Where(x => x.WarehouseOperationExpense.Nomenclature == stBalance.Nomenclature).Select(x => x.WarehouseOperationExpense.Amount).Sum();
				stBalance.Amount += sumOldAmount;
			}
			foreach(var oldNomen in massExpenseOperation) {
				var stBalance = stockBalances.FirstOrDefault(x=> x.Nomenclature == oldNomen.WarehouseOperationExpense.Nomenclature);
				if(stBalance == null) {
					var sum = massExpenseOperation.Where(x => x.WarehouseOperationExpense.Nomenclature == oldNomen.WarehouseOperationExpense.Nomenclature).Select(x => x.WarehouseOperationExpense.Amount).Sum();
					StockBalanceDTO stockBalanceDTO = new StockBalanceDTO();
					stockBalanceDTO.Nomenclature = oldNomen.WarehouseOperationExpense.Nomenclature;
					stockBalanceDTO.Amount = sum;
					stockBalanceDTO.Size = oldNomen.WarehouseOperationExpense.Size;
					stockBalanceDTO.Growth = oldNomen.WarehouseOperationExpense.Growth;
					stockBalances.Add(stockBalanceDTO);
				 }
			}
			return stockBalances;
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
			masEmp.MittensSize = emp.MittensSize;
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
			var ListMassExOperationInProgress = MassExpenseOperation.ToList();

			foreach(var employee in Employees) {

				var properties = typeof(MassExpenseEmployee).GetProperties().Where(x => x.Name.EndsWith("Size") || x.Name.EndsWith("SizeStd"));
				foreach(var propInfoMass in properties) {
					var propInfoEmp = typeof(EmployeeCard).GetProperty(propInfoMass.Name);
					var exist = propInfoEmp.GetValue(employee.EmployeeCard);
					var setValue = propInfoMass.GetValue(employee);
					if(String.IsNullOrWhiteSpace((string)exist) || !String.IsNullOrWhiteSpace((string)setValue)) {
						propInfoEmp.SetValue(employee.EmployeeCard, setValue);
					}
				}

				if(employee.EmployeeCard.Sex == Sex.None)
					employee.EmployeeCard.Sex = employee.Sex;

				if(employee.EmployeeCard.Id == 0)
					uow.Save(employee.EmployeeCard);

				foreach(var nom in ItemsNomenclature) {

					var opMassExpense = ListMassExOperationInProgress.FirstOrDefault(x =>x.EmployeeIssueOperation.Employee.Id == employee.EmployeeCard.Id && x.WarehouseOperationExpense.Nomenclature.Id == nom.Nomenclature.Id);
					if(opMassExpense == null) {

						opMassExpense = new MassExpenseOperation();
						opMassExpense.MassExpenseDoc = this;
						opMassExpense.EmployeeIssueOperation = new EmployeeIssueOperation();
						opMassExpense.WarehouseOperationExpense = new WarehouseOperation();

					}
					else
						ListMassExOperationInProgress.Remove(opMassExpense);

					EmployeeIssueOperation employeeIssueOperation = opMassExpense.EmployeeIssueOperation;
					employeeIssueOperation.Employee = employee.EmployeeCard;
					employeeIssueOperation.Nomenclature = nom.Nomenclature;
					employeeIssueOperation.Size = employee.WearSize;
					employeeIssueOperation.WearGrowth = employee.WearGrowth;
					employeeIssueOperation.Issued = nom.Amount;
					employeeIssueOperation.StartOfUse = this.Date;

					WarehouseOperation warehouseOperation = opMassExpense.WarehouseOperationExpense;
					warehouseOperation.Nomenclature = nom.Nomenclature;
					warehouseOperation.OperationTime = this.Date;
					warehouseOperation.ExpenseWarehouse = this.WarehouseFrom;
					warehouseOperation.ReceiptWarehouse = null;
					var warOper = employee.ListWarehouseOperation.FirstOrDefault(x => x.Nomenclature.Id == nom.Nomenclature.Id);
					warehouseOperation.Size = warOper.Size;
					warehouseOperation.Growth = warOper.Growth;
					warehouseOperation.Amount = nom.Amount;
					warehouseOperation.OperationTime = this.Date;

					employeeIssueOperation.WarehouseOperation = warehouseOperation;

					uow.Save(this);
					uow.Save(warehouseOperation);
					uow.Save(opMassExpense);
				}
			}
			foreach(var operationMassEx in ListMassExOperationInProgress) {
				var empIssueOperation = uow.GetById<EmployeeIssueOperation>(operationMassEx.EmployeeIssueOperation.Id);
				var oper = uow.GetById<WarehouseOperation>(operationMassEx.WarehouseOperationExpense.Id);
				uow.Delete(operationMassEx);
				uow.Delete(empIssueOperation);
				uow.Delete(oper);
			}
					
		}
	}
}
