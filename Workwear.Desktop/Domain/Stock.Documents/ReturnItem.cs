using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Measurement.Domain;
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
		public virtual MeasurementUnit Units {
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
		public virtual string CommentReturn {
			get => commentReturn;
			set { SetField(ref commentReturn, value, () => CommentReturn); }
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

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция возврата на склад")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}

		private EmployeeCard employeeCard;
		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get => employeeCard;
			set => SetField(ref employeeCard, value);
		}

		private EmployeeIssueOperation returnFromEmployeeOperation;
		[Display(Name = "Операция возврата от сотрудника")]
		[IgnoreHistoryTrace]
		public virtual EmployeeIssueOperation ReturnFromEmployeeOperation {
			get => returnFromEmployeeOperation;
			set => SetField(ref returnFromEmployeeOperation, value);
		}

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

		private OverNormOperation returnFromOverNormOperation;
		[Display(Name = "Операция возврата из выдачи вне нормы")]
		[IgnoreHistoryTrace]
		public virtual OverNormOperation ReturnFromOverNormOperation {
			get => returnFromOverNormOperation;
			set => SetField(ref returnFromOverNormOperation, value);
		}

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get { switch(ReturnFrom) {
					case ReturnFrom.Employee : return ReturnFromEmployeeOperation.WearPercent;
					case ReturnFrom.DutyNorm : return ReturnFromDutyNormOperation.WearPercent;
					case ReturnFrom.OverNorm : return ReturnFromOverNormOperation.WearPercent;
					default : return 0;
				}
			}
			set { switch(ReturnFrom) {
					case ReturnFrom.Employee: if(ReturnFromEmployeeOperation.WearPercent != value) {
							ReturnFromEmployeeOperation.WearPercent = value;
							OnPropertyChanged(); } break;
					case ReturnFrom.DutyNorm: if(ReturnFromDutyNormOperation.WearPercent != value) {
							ReturnFromDutyNormOperation.WearPercent = value;
							OnPropertyChanged(); } break;
					case ReturnFrom.OverNorm: if(ReturnFromOverNormOperation.WearPercent != value) {
							ReturnFromOverNormOperation.WearPercent = value;
							OnPropertyChanged(); } break;
				}
			}
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

		#region Не сохраняемые в базу свойства
		public virtual string Title =>
			$"Возврат на склад {Nomenclature?.Name} в количестве {Amount} {Nomenclature?.Type?.Units?.Name}";
		public virtual decimal Total => Cost * Amount;
		public virtual StockPosition StockPosition => new StockPosition(Nomenclature, WarehouseOperation.WearPercent, WearSize, Height, Owner);

		public virtual string BarcodesString => string.Join(
			"\n",
			ReturnBarcodeOperations
				.Select(x => x.Barcode?.Title)
				.Where(x => !string.IsNullOrWhiteSpace(x)));

		public virtual bool CanEditAmount => !ReturnBarcodeOperations.Any();

		private IEnumerable<BarcodeOperation> ReturnBarcodeOperations {
			get {
				switch(ReturnFrom) {
					case ReturnFrom.Employee:
						return ReturnFromEmployeeOperation.BarcodeOperations;
					case ReturnFrom.DutyNorm:
						return ReturnFromDutyNormOperation.BarcodeOperations;
					case ReturnFrom.OverNorm:
						return ReturnFromOverNormOperation.BarcodeOperations;
					default:
						return Enumerable.Empty<BarcodeOperation>();
				}
			}
		}

		public virtual ReturnFrom ReturnFrom {
			get {
				if(IssuedEmployeeOnOperation != null && IssuedDutyNormOnOperation == null && IssuedOverNormOperation == null)
					return ReturnFrom.Employee;
				if(IssuedEmployeeOnOperation == null && IssuedDutyNormOnOperation != null && IssuedOverNormOperation == null)
					return ReturnFrom.DutyNorm;
				if(IssuedEmployeeOnOperation == null && IssuedDutyNormOnOperation == null && IssuedOverNormOperation != null)
					return ReturnFrom.OverNorm;
				throw new InvalidOperationException(
					"Строка документа списания находится в поломанном состоянии. " +
					"Должна быть заполнена хотя бы одна операция.");
			}
		}
		public virtual string ReturnFromText{
			get{
				switch(ReturnFrom) {
					case ReturnFrom.Employee : return $"Сотрудник: {IssuedEmployeeOnOperation.Employee.ShortName}";
					case ReturnFrom.DutyNorm : return $"Дежурное: {IssuedDutyNormOnOperation.DutyNorm.Name}";
					case ReturnFrom.OverNorm : return $"Сверх нормы: {IssuedOverNormOperation.Employee.ShortName}";
					default : return String.Empty;
				}
			}
		}

		//Нужно предварительно заполнять
		private int maxAmount;
		public virtual int MaxAmount {
			get => maxAmount;
			set => SetField(ref maxAmount, value);
		}

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

		/// <summary>
		/// Это ссылка на операцию выдачи по которой был выдан СИЗ
		/// В этом классе используется только для рантайма, в базу не сохраняется, сохраняется внутри операции.
		/// </summary>
		private DutyNormIssueOperation issuedDutyNormOnOperation;
		[Display(Name = "Операция выдачи по дежурной норме")]
		public virtual DutyNormIssueOperation IssuedDutyNormOnOperation {
			get => issuedDutyNormOnOperation ?? ReturnFromDutyNormOperation?.IssuedOperation;
			set => SetField(ref issuedDutyNormOnOperation, value);
		}

		private OverNormOperation issuedOverNormOperation;
		[Display(Name = "Операция выдачи вне нормы")]
		public virtual OverNormOperation IssuedOverNormOperation {
			get => issuedOverNormOperation ?? ReturnFromOverNormOperation?.ReturnFromOperation;
			set => SetField(ref issuedOverNormOperation, value);
		}

		#endregion

		#region Конструкторы

		protected ReturnItem () { }
		public ReturnItem(Return Return, EmployeeIssueOperation issueOperation, int amount, IEnumerable<Barcode> barcodes = null) {
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
			CopyBarcodeOperations(issueOperation.BarcodeOperations, returnFromEmployeeOperation.BarcodeOperations, barcodes,
				x => x.EmployeeIssueOperation = returnFromEmployeeOperation);
			nomenclature = issueOperation.Nomenclature;
			WearSize = issueOperation.WearSize;
			Height = issueOperation.Height;
			employeeCard = issueOperation.Employee;
			this.amount = amount;
		}
		public ReturnItem(Return Return, DutyNormIssueOperation issueOperation, int amount, IEnumerable<Barcode> barcodes = null) {
			document = Return;
			dutyNorm = issueOperation.DutyNorm;
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
			CopyBarcodeOperations(issueOperation.BarcodeOperations, returnFromDutyNormOperation.BarcodeOperations, barcodes,
				x => x.DutyNormIssueOperation = returnFromDutyNormOperation);
			nomenclature = issueOperation.Nomenclature;
			WearSize = issueOperation.WearSize;
			Height = issueOperation.Height;
			this.amount = amount;
		}

		public ReturnItem(Return Return, OverNormOperation issueOperation, int amount, ServiceClaim claim = null, IEnumerable<Barcode> barcodes = null) {
			document = Return;
			serviceClaim = claim;
			issuedOverNormOperation = issueOperation;
			var returningBarcodes = barcodes?.ToList();
			returnFromOverNormOperation = new OverNormOperation {
				Type = issueOperation.Type,
				Employee = issueOperation.Employee,
				Nomenclature = issueOperation.Nomenclature,
				WearSize = issueOperation.WearSize,
				Height = issueOperation.Height,
				WearPercent = issueOperation.WearPercent,
				ReturnFromOperation = issueOperation,
				WarehouseOperation = new WarehouseOperation {
					ReceiptWarehouse = document.Warehouse,
					Amount = amount,
					StockPosition = issueOperation.WarehouseOperation.StockPosition
				}
			};
			warehouseOperation = returnFromOverNormOperation.WarehouseOperation;
			CopyBarcodeOperations(issueOperation.BarcodeOperations, returnFromOverNormOperation.BarcodeOperations, returningBarcodes,
				x => x.OverNormOperation = returnFromOverNormOperation);
			nomenclature = issueOperation.Nomenclature;
			WearSize = issueOperation.WearSize;
			Height = issueOperation.Height;
			employeeCard = issueOperation.Employee;
			this.amount = amount;
		}

		private void CopyBarcodeOperations(
			IEnumerable<BarcodeOperation> sourceOperations,
			ICollection<BarcodeOperation> targetOperations,
			IEnumerable<Barcode> barcodes,
			Action<BarcodeOperation> configureOperation)
		{
			foreach(var sourceOperation in FilterBarcodeOperations(sourceOperations, barcodes)) {
				var newBarcodeOperation = new BarcodeOperation {
					Barcode = sourceOperation.Barcode,
					KitNumber = sourceOperation.KitNumber
				};
				configureOperation(newBarcodeOperation);
				targetOperations.Add(newBarcodeOperation);
				sourceOperation.Barcode.BarcodeOperations.Add(newBarcodeOperation);
			}
		}

		private IEnumerable<BarcodeOperation> FilterBarcodeOperations(
			IEnumerable<BarcodeOperation> sourceOperations,
			IEnumerable<Barcode> barcodes = null)
		{
			var selectedBarcodes = barcodes?.ToList();
			var operations = sourceOperations ?? Enumerable.Empty<BarcodeOperation>();

			return selectedBarcodes == null
				? Enumerable.Empty<BarcodeOperation>()
				: operations.Where(x => selectedBarcodes.Any(b => DomainHelper.EqualDomainObjects(b, x.Barcode)));
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
				case ReturnFrom.OverNorm:
					ReturnFromOverNormOperation.OperationTime = Document.Date;
					ReturnFromOverNormOperation.LastUpdate = DateTime.Now;
					ReturnFromOverNormOperation.WarehouseOperation = WarehouseOperation;
					uow.Save(ReturnFromOverNormOperation);
					break;
				default:
					throw new NotImplementedException();
			}
			ServiceClaim?.Update(uow,this);
		}
		#endregion
	}

	public enum ReturnFrom {
		Employee,
		DutyNorm,
		OverNorm
	}
}
