using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания")]
	public class WriteoffItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Writeoff document;

		[Display(Name = "Документ списания")]
		public virtual Writeoff Document {
			get { return document; }
			set { SetField(ref document, value); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		int amount;

		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		private EmployeeIssueOperation employeeWriteoffOperation;

		[Display(Name = "Операция списания с сотрудника")]
		public virtual EmployeeIssueOperation EmployeeWriteoffOperation
		{
			get { return employeeWriteoffOperation; }
			set { SetField(ref employeeWriteoffOperation, value); }
		}

		private SubdivisionIssueOperation subdivisionWriteoffOperation;

		[Display(Name = "Операция возврата от сотрудника")]
		public virtual SubdivisionIssueOperation SubdivisionWriteoffOperation {
			get { return subdivisionWriteoffOperation; }
			set { SetField(ref subdivisionWriteoffOperation, value); }
		}

		#region Списание со склада

		private Warehouse warehouse;

		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		private WarehouseOperation warehouseOperation;
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation {
			get { return warehouseOperation; }
			set { SetField(ref warehouseOperation, value); }
		}

		#endregion

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

		string aktNumber;
		[Display(Name = "Номер акта")]
		public virtual string AktNumber {
			get { return aktNumber; }
			set { SetField(ref aktNumber, value); }
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

		public virtual string Title {
			get { return String.Format ("Списание {0} в количестве {1} {2}",
				Nomenclature.Name,
				Amount,
				Nomenclature.Type.Units.Name
			);}
		}

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get {
				if(WarehouseOperation != null)
					return WarehouseOperation.WearPercent;
				if(EmployeeWriteoffOperation != null)
					return EmployeeWriteoffOperation.WearPercent;
				if(SubdivisionWriteoffOperation != null)
					return SubdivisionWriteoffOperation.WearPercent;

				throw new InvalidOperationException("Строка документа списания находится в поломанном состоянии. Должна быть заполнена хотя бы одна операция.");
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

		public virtual StockPosition StockPosition => new StockPosition(Nomenclature, Size, WearGrowth, WearPercent);

		public virtual WriteoffFrom WriteoffFrom {
			get {
				if(EmployeeWriteoffOperation != null && WarehouseOperation == null && SubdivisionWriteoffOperation == null)
					return WriteoffFrom.Employye;

				if(EmployeeWriteoffOperation == null && WarehouseOperation == null && SubdivisionWriteoffOperation != null)
					return WriteoffFrom.Subdivision;

				if(EmployeeWriteoffOperation == null && WarehouseOperation != null && SubdivisionWriteoffOperation == null)
					return WriteoffFrom.Warehouse;

				throw new InvalidOperationException("Строка документа списания находится в поломанном состоянии. Должна быть заполнена хотя бы одна операция.");
			}
		}

		#endregion

		#region Не сохраняемые в базу свойства

		private string buhDocument;

		[Display(Name = "Документ бухгалтерского учета")]
		//В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		public virtual string BuhDocument
		{
			get { return buhDocument ?? EmployeeWriteoffOperation?.BuhDocument; }
			set { SetField(ref buhDocument, value); }
		}

		#endregion

		#region Конструкторы

		protected WriteoffItem (){}

		public WriteoffItem(Writeoff writeOff, EmployeeIssueOperation issueOperation, int amount)
		{
			document = writeOff;
			employeeWriteoffOperation = new EmployeeIssueOperation {
				Employee = issueOperation.Employee,
				ProtectionTools = issueOperation.ProtectionTools,
				Returned = amount,
				IssuedOperation = issueOperation,
				OperationTime = document.Date,
				Nomenclature = issueOperation.Nomenclature,
				Size = issueOperation.Size,
				WearGrowth = issueOperation.WearGrowth,
				WearPercent = issueOperation.CalculatePercentWear(document.Date)
			};
			this.nomenclature = issueOperation.Nomenclature ?? throw new ArgumentException("Списывать можно только номенклатуру");
			this.size = issueOperation.Size;
			this.wearGrowth = issueOperation.WearGrowth;
			this.amount = amount;
		}

		public WriteoffItem(Writeoff writeOff, SubdivisionIssueOperation issueOperation, int amount)
		{
			document = writeOff;
			subdivisionWriteoffOperation = new SubdivisionIssueOperation {
				Subdivision = issueOperation.Subdivision,
				Returned = amount,
				IssuedOperation = issueOperation,
				OperationTime = document.Date,
				Nomenclature = issueOperation.Nomenclature,
				Size = issueOperation.Size,
				WearGrowth = issueOperation.WearGrowth,
				WearPercent = issueOperation.CalculatePercentWear(document.Date),
			};
			this.nomenclature = issueOperation.Nomenclature;
			this.size = issueOperation.Size;
			this.wearGrowth = issueOperation.WearGrowth;
			this.amount = amount;
		}

		public WriteoffItem(Writeoff writeOff, StockPosition position, Warehouse warehouse, int amount)
		{
			document = writeOff;
			this.amount = amount;
			this.nomenclature = position.Nomenclature;
			this.size = position.Size;
			this.wearGrowth = position.Growth;
			this.warehouse = warehouse;
			this.warehouseOperation = new WarehouseOperation() {
				Amount = amount,
				Size = position.Size,
				Growth = position.Growth,
				Nomenclature = position.Nomenclature,
				OperationTime = document.Date,
				WearPercent = position.WearPercent,
				ExpenseWarehouse = warehouse
			};
		}

		#endregion

		#region Методы

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			switch(WriteoffFrom) {
				case WriteoffFrom.Employye:
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

	public enum WriteoffFrom
	{
		Employye,
		Subdivision,
		Warehouse
	}
}

