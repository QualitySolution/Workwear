using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Dates;
using QS.Utilities.Numeric;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Operations {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции выдачи по дежурной норме",
		Nominative = "операция выдачи по дежурной норме",
		Genitive ="операции выдачи по дежурной норме"
	)]
	[HistoryTrace]
	public class DutyNormIssueOperation : PropertyChangedBase, IDomainObject, IValidatableObject, IGraphIssueOperation {
		
		/// <summary>
		/// Для создания операций выдачи надо использовать конструктор с BaseParameters
		/// </summary>
		public DutyNormIssueOperation() {
		}
		
		#region Хранимые Свойства
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		private DutyNorm dutyNorm;
		[Display(Name = "Дежурная норма")]
		public virtual DutyNorm DutyNorm {
			get => dutyNorm;
			set => SetField(ref dutyNorm, value);
		}
		
		private DutyNormItem dutyNormItem;
		[Display(Name = "Строка нормы")]
		public virtual DutyNormItem DutyNormItem {
			get => dutyNormItem;
			set => SetField(ref dutyNormItem, value);
		}
		
		DateTime operationTime = DateTime.Now;
		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime {
			get => operationTime;
			set => SetField(ref operationTime, value);
		}

		private ProtectionTools protectionTools;
		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
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
				if (!SetField(ref useAutoWriteoff, value)) return;
				AutoWriteoffDate = value ? ExpiryByNorm : null;
			}
		}

		private DateTime? startOfUse;
		[Display(Name = "Начало использования")]
		public virtual DateTime? StartOfUse {
			get => startOfUse;
			set => SetField(ref startOfUse, value?.Date);
		}

		private DateTime? expiryByNorm;
		[Display(Name = "Износ по норме")]
		public virtual DateTime? ExpiryByNorm {
			get => expiryByNorm;
			set => SetField(ref expiryByNorm, value?.Date);
		}

		private DateTime? autoWriteoffDate;
		[Display(Name = "Дата автосписания")]
		public virtual DateTime? AutoWriteoffDate {
			get => autoWriteoffDate;
			set {
				if(SetField(ref autoWriteoffDate, value?.Date))
					UseAutoWriteoff = value != null;
			}
		}

		private WarehouseOperation warehouseOperation;
		[Display(Name = "Сопутствующая складская операция")]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}
		
		private DutyNormIssueOperation issuedOperation;
		[Display(Name = "Списать операцию выдачи")]
		public virtual DutyNormIssueOperation IssuedOperation {
			get => issuedOperation;
			set => SetField(ref issuedOperation, value);
		}

		IGraphIssueOperation IGraphIssueOperation.IssuedOperation => issuedOperation;

		private bool overrideBefore = false; 
		[Display(Name = "Не учитывать прошлые")]
		public virtual bool OverrideBefore {
			get => overrideBefore;
			set => SetField(ref overrideBefore, value);
		}

		private string comment;
        [Display(Name = "Комментарий")]
        public virtual string Comment {
         	get => comment;
         	set => SetField(ref comment, value);
        }
        #endregion
    
        #region Генерируемые Свойства
        public virtual string Title => Issued > Returned
	        ? $"Выдача {DutyNorm.Name} <= {Issued} х {Nomenclature?.Name ?? ProtectionTools.Name}"
	        : $"Списание {DutyNorm.Name} => {Returned} х {Nomenclature?.Name ?? ProtectionTools.Name}";
        public virtual decimal? LifetimeMonth {
	        get {
		        if(StartOfUse == null || ExpiryByNorm == null)
			        return null;

		        var range = new DateRange(StartOfUse.Value, ExpiryByNorm.Value);
		        return range.Months;
	        }
        }
        #endregion
        
        #region Методы
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if(OperationTime < new DateTime(1990, 1, 1))
				yield return new ValidationResult("Можно сохранить дату операции только после 1990г.");
			if(Issued < 0 || Returned < 0)
				yield return new ValidationResult("Количество не должно быть меньше 0.");
			if(!(Issued > 0 || Returned > 0))
				yield return new ValidationResult("Количество посступления или возврата должно быть больше 0.");
			if(ProtectionTools == null)
				yield return new ValidationResult("Номенклатура нормы должна быть задана.");
			if(Nomenclature == null)
				yield return new ValidationResult("Номенклатура должна быть задана.");
		}
        
        public virtual void Update(ExpenseDutyNormItem item) {
			//Внимание здесь сравниваются даты без времени.
			if (item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			DutyNorm = item.Document.DutyNorm;
			DutyNormItem = item.DutyNormItem;
			Returned = 0;
			WarehouseOperation = item.WarehouseOperation;
			DutyNormItem = DutyNorm.GetItem(ProtectionTools);
			
			RecalculateExpiryByNorm();
        }
        
        public virtual void RecalculateExpiryByNorm(){
	        if(StartOfUse == null)
		        StartOfUse = OperationTime;

	        if(DutyNormItem != null) {
                ExpiryByNorm = DutyNormItem?.CalculateExpireDate(StartOfUse.Value, WearPercent);
                
                if(Issued > DutyNormItem.Amount && DutyNormItem.Amount > 0)
                    ExpiryByNorm = DutyNormItem.CalculateExpireDate(StartOfUse.Value, Issued);
                
                AutoWriteoffDate = UseAutoWriteoff ? ExpiryByNorm : null;
	        }
        }
        #endregion

        #region Статические методы

        public static decimal CalculatePercentWear(DateTime atDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginWearPercent = 0) 
        {
	        if(startOfUse == null || expiryByNorm == null)
		        return 0;
	        if(beginWearPercent >= 1)
		        return beginWearPercent;
			
	        var addPercent = (atDate - startOfUse.Value).TotalDays / (expiryByNorm.Value - startOfUse.Value).TotalDays;
	        if(double.IsNaN(addPercent) || double.IsInfinity(addPercent))
		        return beginWearPercent;

	        return Math.Round(beginWearPercent + (1 - beginWearPercent) * (decimal)addPercent, 2);
        }

        #endregion
	}
}
