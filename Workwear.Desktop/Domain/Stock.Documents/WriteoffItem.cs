using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания",
		Genitive = "строки списания"
		)]
	[HistoryTrace]
	public class WriteoffItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Writeoff document;
		[Display(Name = "Документ списания")]
		[IgnoreHistoryTrace]
		public virtual Writeoff Document {
			get => document;
			set => SetField(ref document, value);
		}

		Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		int amount;
		[Display (Name = "Количество")]
		public virtual int Amount {
			get => amount;
			set { SetField (ref amount, value, () => Amount); }
		}

		private EmployeeIssueOperation employeeWriteoffOperation;
		[Display(Name = "Операция списания с сотрудника")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation EmployeeWriteoffOperation
		{
			get => employeeWriteoffOperation;
			set => SetField(ref employeeWriteoffOperation, value);
		}

		private DutyNormIssueOperation dutyNormWriteOffOperation;

		[Display(Name = "Операция списания с дежурной нормы")]
		[IgnoreHistoryTrace]
		public virtual DutyNormIssueOperation DutyNormWriteOffOperation {
			get=> dutyNormWriteOffOperation;
			set => SetField(ref dutyNormWriteOffOperation, value);
		}
		
		[Display(Name = "Собственник имущества")]
		public virtual Owner Owner {
			get => WarehouseOperation?.Owner;
			set {
				if(WarehouseOperation.Owner != value) {
					WarehouseOperation.Owner = value;
					OnPropertyChanged();
				}
			}
		}

		private CausesWriteOff causesWriteOff;
		[Display(Name = "Причина списания")]
		[IgnoreHistoryTrace]
		public virtual CausesWriteOff CausesWriteOff {
			get => causesWriteOff;
			set => SetField(ref causesWriteOff, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		#region Списание со склада

		private Warehouse warehouse;
		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		private WarehouseOperation warehouseOperation;
		[Display(Name = "Операция на складе")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}

		#endregion
		
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
		#region Вычисляемые
		public virtual string WriteoffFromText{
			get{
				if (Warehouse != null)
					return $"Склад: {Warehouse.Name}";
				if(EmployeeWriteoffOperation != null)
					return $"Сотрудник: {EmployeeWriteoffOperation.Employee.ShortName}";
				if(DutyNormWriteOffOperation != null)
					return $"Дежурное: {DutyNormWriteOffOperation.DutyNorm.Name}";

				return String.Empty;
			}
		}
		
		//Нужно предварительно заполнять
		private int maxAmount;
		public virtual int MaxAmount {
			get => maxAmount;
			set => SetField(ref maxAmount, value);
		}
		public virtual string Title =>
			String.Format ("Списание {0} в количестве {1} {2}",
				Nomenclature?.Name,
				Amount,
				Nomenclature?.Type?.Units?.Name
			);

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get {
				if(WarehouseOperation != null)
					return WarehouseOperation.WearPercent;
				if(EmployeeWriteoffOperation != null)
					return EmployeeWriteoffOperation.WearPercent;
				if(DutyNormWriteOffOperation != null)
					return DutyNormWriteOffOperation.WearPercent;

				throw new InvalidOperationException(
					"Строка документа списания находится в поломанном состоянии. " +
					"Должна быть заполнена хотя бы одна операция.");
			}
			set {
				if(WarehouseOperation != null)
					WarehouseOperation.WearPercent = value;
				if(EmployeeWriteoffOperation != null)
					EmployeeWriteoffOperation.WearPercent = value;
				if(DutyNormWriteOffOperation!=null)
					DutyNormWriteOffOperation.WearPercent = value;
			}
		}

		public virtual StockPosition StockPosition => new StockPosition(
			Nomenclature, 
			WearPercent, 
			WearSize, 
			Height,
			Owner);

		public virtual WriteoffFrom WriteoffFrom {
			get {
				if(EmployeeWriteoffOperation != null && WarehouseOperation == null && DutyNormWriteOffOperation == null)
					return WriteoffFrom.Employee;

				if(EmployeeWriteoffOperation == null && WarehouseOperation != null && DutyNormWriteOffOperation == null)
					return WriteoffFrom.Warehouse;
				if(EmployeeWriteoffOperation == null && WarehouseOperation == null && DutyNormWriteOffOperation != null)
					return WriteoffFrom.DutyNorm;

				throw new InvalidOperationException(
					"Строка документа списания находится в поломанном состоянии. " +
					"Должна быть заполнена хотя бы одна операция.");
			}
		}

		#endregion
		#region Конструкторы
		protected WriteoffItem (){}
		public WriteoffItem(Writeoff writeOff, EmployeeIssueOperation issueOperation, int amount) {
			document = writeOff;
			employeeWriteoffOperation = new EmployeeIssueOperation {
				Employee = issueOperation.Employee,
				ProtectionTools = issueOperation.ProtectionTools,
				Returned = amount,
				IssuedOperation = issueOperation,
				OperationTime = document.Date,
				Nomenclature = issueOperation.Nomenclature,
				WearSize = issueOperation.WearSize,
				Height = issueOperation.Height,
				WearPercent = issueOperation.CalculatePercentWear(document.Date)
			};
			nomenclature = issueOperation.Nomenclature;
			WearSize = issueOperation.WearSize;
			Height = issueOperation.Height;
			this.amount = amount;
		}

		public WriteoffItem(Writeoff writeoff, DutyNormIssueOperation issueOperation, int amount) {
			document = writeoff;
			dutyNormWriteOffOperation = new DutyNormIssueOperation {
				DutyNorm = issueOperation.DutyNorm,
				ProtectionTools = issueOperation.ProtectionTools,
				Returned = amount,
				IssuedOperation = issueOperation,
				OperationTime = document.Date,
				Nomenclature = issueOperation.Nomenclature,
				WearSize = issueOperation.WearSize,
				Height = issueOperation.Height,
				WearPercent = issueOperation.CalculatePercentWear(document.Date)
			};
			nomenclature = issueOperation.Nomenclature;
			WearSize = issueOperation.WearSize;
			Height = issueOperation.Height;
			this.amount = amount;
		}
		
		public WriteoffItem(Writeoff writeOff, StockPosition position, Warehouse warehouse, int amount) {
			document = writeOff;
			this.amount = amount;
			nomenclature = position.Nomenclature;
			WearSize = position.WearSize;
			Height = position.Height;
			this.warehouse = warehouse;
			warehouseOperation = new WarehouseOperation() {
				Amount = amount,
				WearSize = position.WearSize,
				Height = position.Height,
				Nomenclature = position.Nomenclature,
				OperationTime = document.Date,
				WearPercent = position.WearPercent,
				ExpenseWarehouse = warehouse
			};
			Owner = position.Owner;
		}
		#endregion
		#region Методы
		public virtual void UpdateOperations(IUnitOfWork uow) {
			switch(WriteoffFrom) {
				case WriteoffFrom.Employee:
					EmployeeWriteoffOperation.Update(uow, this);
					break;
				case WriteoffFrom.Warehouse:
					WarehouseOperation.Update(uow, this);
					break;
				case WriteoffFrom.DutyNorm:
					DutyNormWriteOffOperation.Update(uow, this);
					break;
				default:
					throw new NotImplementedException();
			}
		}
		
		public virtual bool CanSetOwner => WarehouseOperation != null;
		#endregion
	}
	public enum WriteoffFrom {
		Employee,
		Warehouse,
		DutyNorm
	}
}

