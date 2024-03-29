﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities.Dates;
using QS.Utilities.Numeric;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools;

namespace Workwear.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции выдачи на подразделение",
		Nominative = "операция выдачи на подразделение",
		Genitive ="операции выдачи на подразделений"
	)]
	[HistoryTrace]
	public class SubdivisionIssueOperation : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		DateTime operationTime = DateTime.Now;
		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime {
			get => operationTime;
			set => SetField(ref operationTime, value);
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		SubdivisionPlace subdivisionPlace;
		[Display(Name = "Размещение в подразделении")]
		public virtual SubdivisionPlace SubdivisionPlace {
			get => subdivisionPlace;
			set => SetField(ref subdivisionPlace, value);
		}

		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
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

		private decimal wearPercent;
		/// <summary>
		/// Процент износа не может быть меньше нуля.
		/// Новый СИЗ имеет 0%, далее нарастает при использовании.
		/// Процент хранится в виде коэффициента, то есть значение 1 = 100%
		/// И в базе ограничение на 3 хранимых символа поэтому максимальное значение 9.99
		/// </summary>
		/// <value>The wear percent.</value>
		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => wearPercent;
			set => SetField(ref wearPercent, value.Clamp(0m, 9.99m));
		}

		private int issued;
		[Display(Name = "Выдано")]
		public virtual int Issued {
			get => issued;
			set => SetField(ref issued, value);
		}

		private int returned;
		[Display(Name = "Возвращено")]
		public virtual int Returned {
			get => returned;
			set => SetField(ref returned, value);
		}

		private bool useAutoWriteoff = true;
		[Display(Name = "Использовать автосписание")]
		public virtual bool UseAutoWriteoff {
			get => useAutoWriteoff;
			set {
				if (SetField(ref useAutoWriteoff, value))
					if (value)
						AutoWriteoffDate = ExpiryOn;
					else
						AutoWriteoffDate = null;
			}
		}

		private DateTime? startOfUse;
		[Display(Name = "Начало использования")]
		public virtual DateTime? StartOfUse {
			get => startOfUse;
			set => SetField(ref startOfUse, value);
		}

		private DateTime? expiryOn;
		[Display(Name = "Износ по норме")]
		public virtual DateTime? ExpiryOn {
			get => expiryOn;
			set => SetField(ref expiryOn, value);
		}

		private DateTime? autoWriteoffDate;
		[Display(Name = "Дата автосписания")]
		public virtual DateTime? AutoWriteoffDate {
			get => autoWriteoffDate;
			set => SetField(ref autoWriteoffDate, value);
		}

		private SubdivisionIssueOperation issuedOperation;
		[Display(Name = "Операция выдачи")]
		public virtual SubdivisionIssueOperation IssuedOperation {
			get => issuedOperation;
			set => SetField(ref issuedOperation, value);
		}

		private WarehouseOperation warehouseOperation;
		[Display(Name = "Сопутствующая складская операция")]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}

		/// <summary>
		/// Для создания операций выдачи надо использовать конструктор с BaseParameters
		/// </summary>
		public SubdivisionIssueOperation() { }
		public SubdivisionIssueOperation(BaseParameters baseParameters) {
			useAutoWriteoff = baseParameters.DefaultAutoWriteoff;
		}
		#region Расчетные
		public virtual string Title => Issued > Returned
			? $"Выдача {Subdivision.Code} <= {Issued} х {Nomenclature.Name}"
			: $"Списание {Subdivision.Code} => {Returned} х {Nomenclature.Name}";

		public virtual decimal? LifetimeMonth {
			get {
				if(StartOfUse == null || ExpiryOn == null)
					return null;

				var range = new DateRange(StartOfUse.Value, ExpiryOn.Value);
				return range.Months;
			}
		}
		#endregion
		#region Методы
		public virtual decimal CalculatePercentWear(DateTime atDate) => 
			CalculatePercentWear(atDate, StartOfUse, ExpiryOn, WearPercent);

		public static decimal CalculatePercentWear(DateTime atDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginWearPercent) {
			if(startOfUse == null || expiryByNorm == null)
				return 0;

			var addPercent = (atDate - startOfUse.Value).TotalDays / (expiryByNorm.Value - startOfUse.Value).TotalDays;
			if(double.IsNaN(addPercent) || double.IsInfinity(addPercent))
				return beginWearPercent;

			return beginWearPercent + (decimal)addPercent;
		}

		public virtual decimal CalculateDepreciationCost(DateTime atDate) => 
			CalculateDepreciationCost(atDate, StartOfUse, ExpiryOn, WarehouseOperation.Cost);

		public static decimal CalculateDepreciationCost(DateTime atDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginCost) {
			if(startOfUse == null || expiryByNorm == null)
				return 0;

			var removePercent = (atDate - startOfUse.Value).TotalDays / (expiryByNorm.Value - startOfUse.Value).TotalDays;
			if(double.IsNaN(removePercent) || double.IsInfinity(removePercent))
				return beginCost;

			return (beginCost - beginCost * (decimal)removePercent).Clamp(0, decimal.MaxValue);
		}
		#endregion
		#region Методы обновленя операций

		public virtual void Update(IUnitOfWork uow, IInteractiveQuestion askUser, ExpenseItem item) {
			//Внимание здесь сравниваются даты без времени.
			if (item.ExpenseDoc.Date.Date != OperationTime.Date)
				OperationTime = item.ExpenseDoc.Date;

			Subdivision = item.ExpenseDoc.Subdivision;
			SubdivisionPlace = item.SubdivisionPlace;
			Nomenclature = item.Nomenclature;
			WearSize = item.WearSize;
			Height = item.Height;
			WearPercent = item.WarehouseOperation.WearPercent;
			Issued = item.Amount;
			Returned = 0;
			IssuedOperation = null;
			WarehouseOperation = item.WarehouseOperation;
		}

		public virtual void Update(IUnitOfWork uow, IInteractiveQuestion askUser, IncomeItem item) {
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			Subdivision = item.Document.Subdivision;
			Nomenclature = item.Nomenclature;
			WearSize = item.WearSize;
			Height = item.Height;
			WearPercent = item.WearPercent;
			Issued = 0;
			Returned = item.Amount;
			WarehouseOperation = item.WarehouseOperation;
			IssuedOperation = item.IssuedSubdivisionOnOperation;
			ExpiryOn = null;
			AutoWriteoffDate = null;
		}

		public virtual void Update(IUnitOfWork uow, WriteoffItem item) {
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;
			
			Nomenclature = item.Nomenclature;
			Issued = 0;
			Returned = item.Amount;
			WarehouseOperation = item.WarehouseOperation;
			ExpiryOn = null;
			AutoWriteoffDate = null;
			WearSize = item.WearSize;
			Height = item.Height;
		}

		#endregion
	}
}
