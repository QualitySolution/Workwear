using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Utilities.Numeric;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Operations {
	public class DutyNormIssueOperation : PropertyChangedBase, IDomainObject, IValidatableObject {
		#region Свойства
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

		private string comment;
        [Display(Name = "Комментарий")]
        public virtual string Comment {
         	get => comment;
         	set => SetField(ref comment, value);
        }
        #endregion
//Разбор опраций        
        /// <summary>
        /// Для создания операций выдачи надо использовать конструктор с BaseParameters
        /// </summary>
        public DutyNormIssueOperation() {
        }

        
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			throw new NotImplementedException();
		}

		public virtual void Update(ExpenseDutyNornItem item) {
			//Внимание здесь сравниваются даты без времени.
			if (item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			DutyNorm = item.Document.DutyNorm;
			DutyNormItem = item.DutyNormItem;
			Nomenclature = item.Nomenclature;
			WearSize = item.WearSize;
			Height = item.Height;
			WearPercent = item.WarehouseOperation.WearPercent;
			Issued = item.Amount;
			Returned = 0;
			WarehouseOperation = item.WarehouseOperation;
			ProtectionTools = item.ProtectionTools;
		}
	}
}
