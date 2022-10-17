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

		private SubdivisionIssueOperation subdivisionWriteoffOperation;
		[Display(Name = "Операция возврата от сотрудника")]
		[IgnoreHistoryTrace]
		public virtual SubdivisionIssueOperation SubdivisionWriteoffOperation {
			get => subdivisionWriteoffOperation;
			set => SetField(ref subdivisionWriteoffOperation, value);
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

		string aktNumber;
		[Display(Name = "Номер акта")]
		public virtual string AktNumber {
			get => aktNumber;
			set => SetField(ref aktNumber, value);
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
		#region Вычисляемые
		public virtual string LastOwnText{
			get{
				if (Warehouse != null)
					return Warehouse.Name;
				if(EmployeeWriteoffOperation != null)
					return EmployeeWriteoffOperation.Employee.ShortName;
				if(SubdivisionWriteoffOperation != null)
					return SubdivisionWriteoffOperation.Subdivision.Name;

				return String.Empty;
			}
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
				if(SubdivisionWriteoffOperation != null)
					return SubdivisionWriteoffOperation.WearPercent;

				throw new InvalidOperationException(
					"Строка документа списания находится в поломанном состоянии. " +
					"Должна быть заполнена хотя бы одна операция.");
			}
			set {
				if(WarehouseOperation != null)
					WarehouseOperation.WearPercent = value;
				if(EmployeeWriteoffOperation != null)
					EmployeeWriteoffOperation.WearPercent = value;
				if(SubdivisionWriteoffOperation != null)
					SubdivisionWriteoffOperation.WearPercent = value;
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
				if(EmployeeWriteoffOperation != null && WarehouseOperation == null && SubdivisionWriteoffOperation == null)
					return WriteoffFrom.Employee;

				if(EmployeeWriteoffOperation == null && WarehouseOperation == null && SubdivisionWriteoffOperation != null)
					return WriteoffFrom.Subdivision;

				if(EmployeeWriteoffOperation == null && WarehouseOperation != null && SubdivisionWriteoffOperation == null)
					return WriteoffFrom.Warehouse;

				throw new InvalidOperationException(
					"Строка документа списания находится в поломанном состоянии. " +
					"Должна быть заполнена хотя бы одна операция.");
			}
		}

		#endregion
		#region Не сохраняемые в базу свойства
		private string buhDocument;
		[Display(Name = "Документ бухгалтерского учета")]
		//В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		public virtual string BuhDocument {
			get => buhDocument ?? EmployeeWriteoffOperation?.BuhDocument;
			set => SetField(ref buhDocument, value);
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

		public WriteoffItem(Writeoff writeOff, SubdivisionIssueOperation issueOperation, int amount) {
			document = writeOff;
			subdivisionWriteoffOperation = new SubdivisionIssueOperation {
				Subdivision = issueOperation.Subdivision,
				Returned = amount,
				IssuedOperation = issueOperation,
				OperationTime = document.Date,
				Nomenclature = issueOperation.Nomenclature,
				WearSize = issueOperation.WearSize,
				Height = issueOperation.Height,
				WearPercent = issueOperation.CalculatePercentWear(document.Date),
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
				case WriteoffFrom.Subdivision:
					SubdivisionWriteoffOperation.Update(uow, this);
					break;
				case WriteoffFrom.Warehouse:
					WarehouseOperation.Update(uow, this);
					break;
				default:
					throw new NotImplementedException();
			}
		}
		#endregion
	}
	public enum WriteoffFrom {
		Employee,
		Subdivision,
		Warehouse
	}
}

