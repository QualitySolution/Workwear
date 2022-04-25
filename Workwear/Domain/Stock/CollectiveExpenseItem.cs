using System;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Statements;
using workwear.Repository.Stock;
using workwear.Tools;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки коллективной выдачи",
		Nominative = "строка коллективной выдачи",
		Genitive = "строки коллективной выдачи"
		)]
	[HistoryTrace]
	public class CollectiveExpenseItem : PropertyChangedBase, IDomainObject
	{
		#region Сохраняемые свойства

		public virtual int Id { get; set; }

		CollectiveExpense document;

		[Display (Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual CollectiveExpense Document {
			get { return document; }
			set { SetField (ref document, value, () => Document); }
		}

		EmployeeCard employee;

		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get { return employee; }
			set { SetField(ref employee, value, () => Employee); }
		}

		ProtectionTools protectionTools;

		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get { return protectionTools ?? EmployeeIssueOperation?.ProtectionTools; }
			set { SetField(ref protectionTools, value, () => ProtectionTools); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		private IssuanceSheetItem issuanceSheetItem;
		[Display(Name = "Строка ведомости")]
		public virtual IssuanceSheetItem IssuanceSheetItem {
			get => issuanceSheetItem;
			set => SetField(ref issuanceSheetItem, value);
		}

		int amount;

		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		private EmployeeIssueOperation employeeIssueOperation;

		[Display(Name = "Операция выдачи сотруднику")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get { return employeeIssueOperation; }
			set { SetField(ref employeeIssueOperation, value); }
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get { return warehouseOperation; }
			set { SetField(ref warehouseOperation, value);}
		}

		string size;

		[Display(Name = "Размер")]
		public virtual string Size {
			get { return size; }
			set { SetField(ref size, value, () => Size); }
		}

		string wearGrowth;

		[Display(Name = "Рост одежды")]
		public virtual string WearGrowth {
			get { return wearGrowth; }
			set { SetField(ref wearGrowth, value, () => WearGrowth); }
		}

		#endregion

		#region Не сохраняемые в базу свойства

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => WarehouseOperation.WearPercent;
			set => WarehouseOperation.WearPercent = value;
		}

		private EmployeeCardItem employeeCardItem;

		public virtual EmployeeCardItem EmployeeCardItem {
			get => employeeCardItem;
			set => employeeCardItem = value;
		}

		public virtual StockPosition StockPosition {
			get {
				return new StockPosition(Nomenclature, Size, WearGrowth, WearPercent);
			}
			set {
				Nomenclature = value.Nomenclature;
				Size = value.Size;
				WearGrowth = value.Growth;
				WearPercent = value.WearPercent;
			}
		}

		StockBalanceDTO stockBalanceSetter;
		public virtual StockBalanceDTO StockBalanceSetter {
			get {
				return stockBalanceSetter ?? new StockBalanceDTO {Nomenclature = Nomenclature, Growth = WearGrowth, Size = Size, WearPercent = WearPercent } ;
			}
			set {
				stockBalanceSetter = value;
				Nomenclature = value.Nomenclature;
				Size = value.Size;
				WearGrowth = value.Growth;
				WearPercent = value.WearPercent;
			}
		}
		#endregion

		#region Расчетные свойства
		public virtual string Title {
			get { return String.Format ("Выдача со склада {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units?.Name
			).TrimEnd();}
		}

		#endregion

		#region Функции

		public virtual void UpdateOperations(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser)
		{
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);

			if (EmployeeIssueOperation == null) {
				EmployeeIssueOperation = new EmployeeIssueOperation();
			}

			EmployeeIssueOperation.Update(uow, baseParameters, askUser, this);
												
			uow.Save(EmployeeIssueOperation);
		}

		#endregion

	}
}

