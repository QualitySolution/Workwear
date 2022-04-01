using System;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Sizes;
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

		private CollectiveExpense document;
		[Display (Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual CollectiveExpense Document {
			get => document;
			set { SetField (ref document, value, () => Document); }
		}

		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get => employee;
			set { SetField(ref employee, value, () => Employee); }
		}

		private ProtectionTools protectionTools;
		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools ?? EmployeeIssueOperation?.ProtectionTools;
			set { SetField(ref protectionTools, value, () => ProtectionTools); }
		}

		private Nomenclature nomenclature;
		[Display (Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		private IssuanceSheetItem issuanceSheetItem;
		[Display(Name = "Строка ведомости")]
		public virtual IssuanceSheetItem IssuanceSheetItem {
			get => issuanceSheetItem;
			set => SetField(ref issuanceSheetItem, value);
		}

		private int amount;
		[Display (Name = "Количество")]
		public virtual int Amount {
			get => amount;
			set { SetField (ref amount, value, () => Amount); }
		}

		private EmployeeIssueOperation employeeIssueOperation;
		[Display(Name = "Операция выдачи сотруднику")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation EmployeeIssueOperation
		{
			get => employeeIssueOperation;
			set => SetField(ref employeeIssueOperation, value);
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}
		private Size wearSize;
		[Display(Name = "Размер")]
		public virtual Size WearSize {
			get => wearSize;
			set => SetField(ref wearSize, value);
		}
		private Size height;
		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => SetField(ref height, value);
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
			get => new StockPosition(Nomenclature, WearPercent, WearSize, Height);
			set {
				Nomenclature = value.Nomenclature;
				WearSize = value.WearSize;
				Height = value.Height;
				WearPercent = value.WearPercent;
			}
		}

		private StockBalanceDTO stockBalanceSetter;

		public virtual StockBalanceDTO StockBalanceSetter
		{
			get => stockBalanceSetter ??
			       new StockBalanceDTO
				       {Nomenclature = Nomenclature, WearPercent = WearPercent, WearSize = WearSize, Height = Height};
			set
			{
				stockBalanceSetter = value;
				Nomenclature = value.Nomenclature;
				WearSize = value.WearSize;
				Height = value.Height;
				WearPercent = value.WearPercent;
			}
		}

		#endregion
		#region Расчетные свойства
		public virtual string Title =>
			String.Format ("Выдача со склада {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units?.Name
			).TrimEnd();

		#endregion
		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser) {
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

