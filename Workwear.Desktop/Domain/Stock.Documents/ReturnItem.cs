using System;
using System.ComponentModel.DataAnnotations;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки возврата",
		Nominative = "строка возврата",
		Genitive = "строки возврата"
		)]
	[HistoryTrace]
	public class ReturnItem : PropertyChangedBase, IDomainObject , IDocItemSizeInfo
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Return document;
		[Display(Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Return Document {
			get => document;
			set => SetField(ref document, value);
		}

		Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}
		
		[Display (Name = "Наименование")]
		public virtual string ItemName {
			get => nomenclature?.Name ?? IssuedEmployeeOnOperation?.ProtectionTools.Name;
		}
		
		[Display(Name = "Тип Роста")]
		public virtual SizeType HeightType {
			get => nomenclature?.Type.HeightType;
		}
		
		[Display(Name = "Тип размера одежды")]
		public virtual SizeType WearSizeType {
			get => nomenclature?.Type.SizeType;
		}
		
		[Display(Name = "Единица измерения")]
		public virtual MeasurementUnits Units {
			get => nomenclature?.Type.Units ?? IssuedEmployeeOnOperation?.ProtectionTools?.Type.Units;
		}
		private int amount;
		[Display (Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get => amount;
			set { SetField (ref amount, value, () => Amount); }
		}

		private decimal cost;
		[Display (Name = "Цена")]
		[PropertyChangedAlso("Total")]
		public virtual decimal Cost {
			get => cost;
			set { SetField (ref cost, value, () => Cost); }
		}
		
		private string commentReturn;
		[Display(Name = "Отметка о возврате")]
		public virtual string СommentReturn {
			get => commentReturn;
			set { SetField(ref commentReturn, value, () => СommentReturn); }
		}

		private EmployeeIssueOperation returnFromEmployeeOperation;
		[Display(Name = "Операция возврата от сотрудника")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation ReturnFromEmployeeOperation {
			get => returnFromEmployeeOperation;
			set => SetField(ref returnFromEmployeeOperation, value);
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
			set => wearSize = value;
		}
		private Size height;
		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => height = value;
		}

		[Display(Name = "Собственник имущества")]
		public virtual Owner Owner {
			get => WarehouseOperation.Owner;
			set {
				if(WarehouseOperation.Owner != value) {
					WarehouseOperation.Owner = value;
					OnPropertyChanged();
				}
			}
		}
		private EmployeeCard employeeCard;
		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get => employeeCard;
			set => SetField(ref employeeCard, value);
		}

		#region Возврат с дежурной нормы

		private DutyNorm dutyNorm;
		[Display(Name = "Дежурная норма")]
		public virtual DutyNorm DutyNorm {
			get => dutyNorm;
			set => SetField (ref dutyNorm, value);
		}
		
		private DutyNormIssueOperation returnFromDutyNormOperation;
		[IgnoreHistoryTrace]
		[Display(Name = "Операция возврата с дежурной нормы")]
		public virtual DutyNormIssueOperation ReturnFromDutyNormOperation {
			get => returnFromDutyNormOperation;
			set => SetField(ref returnFromDutyNormOperation, value);
		}

		#endregion

		#region Возврат со стирки
		private ServiceClaim serviceClaim;
		[Display(Name="Завершаемая заявка на обслуживание")]
		public virtual ServiceClaim ServiceClaim {
			get=> serviceClaim;
			set=>SetField(ref serviceClaim, value);
		}
		

		#endregion

		#endregion
		#region Расчетные
		public virtual string Title =>
			$"Возврат на склад {Nomenclature?.Name} в количестве {Amount} {Nomenclature?.Type?.Units?.Name}";
		public virtual decimal Total => Cost * Amount;
		public virtual StockPosition StockPosition => new StockPosition(Nomenclature, WarehouseOperation.WearPercent, WearSize, Height, Owner);

		public virtual ReturnFrom ReturnFrom {
			get {
				if(IssuedEmployeeOnOperation != null && IssuedDutyNormOnOperation == null)
					return ReturnFrom.Employee;
				if(IssuedEmployeeOnOperation == null && IssuedDutyNormOnOperation != null)
					return ReturnFrom.DutyNorm;
				if(ServiceClaim != null)
					return ReturnFrom.Claim;
				throw new InvalidOperationException(
					"Строка документа списания находится в поломанном состоянии. " +
					"Должна быть заполнена хотя бы одна операция.");
			}
		}
		public virtual string LastOwnText{
			get{
				if(IssuedEmployeeOnOperation != null)
					return IssuedEmployeeOnOperation.Employee.ShortName;
				if(IssuedDutyNormOnOperation != null)
					return IssuedDutyNormOnOperation.DutyNorm.Name;

				return String.Empty;
			}
		}
		
		//Нужно предварительно заполнять
		private int maxAmount;
		public virtual int MaxAmount {
			get => maxAmount;
			set => SetField(ref maxAmount, value);
		}
		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get {
				if(IssuedEmployeeOnOperation != null)
					return IssuedEmployeeOnOperation.WearPercent;
				if(IssuedDutyNormOnOperation != null)
					return IssuedDutyNormOnOperation.WearPercent;

				throw new InvalidOperationException(
					"Строка документа списания находится в поломанном состоянии. " +
					"Должна быть заполнена хотя бы одна операция.");
			}
			set {
				if(IssuedEmployeeOnOperation != null)
					IssuedEmployeeOnOperation.WearPercent = value;
				if(IssuedDutyNormOnOperation!=null)
					IssuedDutyNormOnOperation.WearPercent = value;
			}
		}
		
		#endregion
		
		#region Не сохраняемые в базу свойства
		private EmployeeIssueOperation issuedEmployeeOnOperation;
		/// <summary>
		/// Это ссылка на операцию выдачи по которой был выдан сотруднику поступивший от него СИЗ
		/// В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		/// </summary>
		[Display(Name = "Операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation IssuedEmployeeOnOperation {
			get => issuedEmployeeOnOperation ?? ReturnFromEmployeeOperation?.IssuedOperation;
			set => SetField(ref issuedEmployeeOnOperation, value);
		}

		private DutyNormIssueOperation issuedDutyNormOnOperation;

		[Display(Name = "Операция выдачи по дежурной норме")]
		public virtual DutyNormIssueOperation IssuedDutyNormOnOperation {
			get=>issuedDutyNormOnOperation?? ReturnFromDutyNormOperation?.IssuedOperation;
			set => SetField(ref issuedDutyNormOnOperation, value);
		}

		#endregion

		#region Конструкторы
		
		protected ReturnItem () { }
		public ReturnItem(Return Return, EmployeeIssueOperation issueOperation, int amount) {
			document = Return;
			returnFromEmployeeOperation = new EmployeeIssueOperation{
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
			employeeCard = issueOperation.Employee;
			this.amount = amount;
		}
		public ReturnItem(Return Return, DutyNormIssueOperation issueOperation, int amount) {
			document = Return;
			returnFromDutyNormOperation = new DutyNormIssueOperation {
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

		public ReturnItem(Return Return, ServiceClaim claim, int amount) {
			document = Return;
			this.nomenclature = claim.Barcode.Nomenclature;
			this.wearSize = claim.Barcode.Size;
			this.height = claim.Barcode.Height;
			this.amount = amount;
		}

		#endregion
		
		
		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow) {
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
			switch(ReturnFrom) {
				case ReturnFrom.Employee:
					ReturnFromEmployeeOperation.Update(uow,this);
					break;
				case ReturnFrom.DutyNorm:
					ReturnFromDutyNormOperation.Update(uow,this);
					break;
				default:
					throw new NotImplementedException();
			}
		}
		#endregion
	}

	public enum ReturnFrom {
		Employee,
		DutyNorm,
		Claim
	}
}
