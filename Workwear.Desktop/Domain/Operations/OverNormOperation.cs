using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Numeric;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Operations {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции выдачи вне нормы",
		Nominative = "операция выдачи вне нормы"
	)]
	[HistoryTrace]
	public class OverNormOperation : PropertyChangedBase, IDomainObject 
	{
		#region Properties
		public virtual int Id { get; set; }
		
		DateTime operationTime = DateTime.Now;
		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime 
		{
			get => operationTime;
			set => SetField(ref operationTime, value);
		}
		
		DateTime lastUpdate = DateTime.Now;
		[Display(Name = "Время последнего обновления операции")]
		public virtual DateTime LastUpdate 
		{
			get => lastUpdate;
			set => SetField(ref lastUpdate, value);
		}

		private OverNormType type;
		[Display(Name = "Тип операции выдачи вне нормы")]
		public virtual OverNormType Type 
		{
			get => type;
			set => SetField(ref type, value);
		}

		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee 
		{
			get => employee;
			set => SetField(ref employee, value);
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
		/// Процент хранится в виде коэффициента, то есть значение 1 = 100%
		/// И в базе ограничение на 3 хранимых символа поэтому максимальное значение 9.99
		/// </summary>
		/// <value>The wear percent.</value>
		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => wearPercent;
			set => SetField(ref wearPercent, value.Clamp(0m, 9.99m));
		}
		
		private WarehouseOperation warehouseOperation;
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation 
		{
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}
		
		private EmployeeIssueOperation substitutedIssueOperation;
		[Display(Name = "Заменяемая выдача у сотрудника")]
		public virtual EmployeeIssueOperation SubstitutedIssueOperation
		{
			get => substitutedIssueOperation;
			set => SetField(ref substitutedIssueOperation, value);
		}
		
		private OverNormOperation returnFromOperation;
		[Display(Name = "Списываемая операция выдачи вне нормы")]
		public virtual OverNormOperation ReturnFromOperation 
		{
			get => returnFromOperation;
			set => SetField(ref returnFromOperation, value);
		}

		private IList<BarcodeOperation> barcodeOperations = new List<BarcodeOperation>();
		[Display(Name = "Операции со штрихкодами")]
		public virtual IList<BarcodeOperation> BarcodeOperations 
		{
			get => barcodeOperations;
			set => SetField(ref barcodeOperations, value);
		}
		
		private IList<Barcode> barcodes = new List<Barcode>();
		[Display(Name = "Операции со штрихкодами")]
		public virtual IList<Barcode> Barcodes 
		{
			get => barcodes;
			set => SetField(ref barcodes, value);
		}
		#endregion

		#region Not Mapped Propertis
		public virtual string Title => $"Операция выдачи выдачи вне нормы ({Type.GetAttribute<DisplayAttribute>().Name}) {WarehouseOperation.Nomenclature.Name} в количестве {WarehouseOperation.Amount}";
		#endregion
	}
	
	public enum OverNormType
	{
		[Display(Name = "Разовая")]
		Simple,
		[Display(Name = "Подменная")]
		Substitute,
		[Display(Name = "Гостевая")]
		Guest
	}
}
