using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи дежурной нормы",
		Nominative = "строка выдачи дежурной нормы",
		Genitive = "строки выдачи дежурной нормы"
		)]
	[HistoryTrace]
	public class ExpenseDutyNornItem : PropertyChangedBase, IDomainObject
	{
		#region Генерирумые Сввойства
		public virtual string Title => $"Строка {Nomenclature?.Name ?? ProtectionTools.Name} в {Document.Title}";
		#endregion
		
		#region Хранимые свойства

		public virtual int Id { get; set; }

		private ExpenseDutyNorn document;
		[Display (Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual ExpenseDutyNorn Document {
			get => document;
			set { SetField (ref document, value); }
		}

		private ProtectionTools protectionTools;
		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		private Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value); }
		}

		private int amount;
		[Display (Name = "Количество")]
		public virtual int Amount {
			get => amount;
			set { SetField (ref amount, value); }
		}
		
		private DutyNormIssueOperation operation = new DutyNormIssueOperation();
		[Display(Name = "Операция")]
		[IgnoreHistoryTrace]
		public virtual DutyNormIssueOperation Operation {
			get => operation;
			set => SetField(ref operation, value);
		}
		
		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Складская операция")]
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
//не думаю, что нужно хранить в базе, это поле хранися в операции
		private DutyNormItem dutyNormItem;
		[Display(Name = "Строка дежурной нормы")]
		[IgnoreHistoryTrace]
		public virtual DutyNormItem DutyNormItem {
			get => dutyNormItem;
			set => SetField(ref dutyNormItem, value);
		}

		public virtual void UpdateOperation(IUnitOfWork uow) {
			if (Operation == null) 
				Operation = new DutyNormIssueOperation();

			Operation.Update(this);
			uow.Save(Operation);
			
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
		}
	}
}
