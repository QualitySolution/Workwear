using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи",
		Nominative = "строка выдачи",
		Genitive = "строки выдачи"
		)]
	[HistoryTrace]
	public class ExpenseDutyNornItem : PropertyChangedBase, IDomainObject
	{
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

		[Display(Name = "Строка дежурной нормы")]
		public virtual DutyNormItem DutyNormItem => Operation.DutyNormItem;

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
