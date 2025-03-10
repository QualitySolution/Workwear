using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;

namespace Workwear.Domain.Stock.Documents {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи по дежурной нормы",
		PrepositionalPlural = "строках выдачи по дежурной норме",
		Nominative = "строка выдачи дежурной нормы",
		Genitive = "строки выдачи дежурной нормы"
		)]
	[HistoryTrace]
	public class ExpenseDutyNormItem : PropertyChangedBase, IDomainObject
	{
		#region Хранимые свойства

		public virtual int Id { get; set; }

		private ExpenseDutyNorm document;
		[Display (Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual ExpenseDutyNorm Document {
			get => document;
			set { SetField (ref document, value); }
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
		
		private IssuanceSheetItem issuanceSheetItem;

		[Display(Name = "Строка ведомости")]
		public virtual IssuanceSheetItem IssuanceSheetItem {
			get => issuanceSheetItem;
			set => SetField(ref issuanceSheetItem, value);
		}
		#endregion

		#region Расчётные свойства и пробросы
		public virtual string Title => $"Строка {Nomenclature?.Name ?? ProtectionTools.Name} в {Document.Title}";
		
		[Display(Name = "Складская позиция")]
        [IgnoreHistoryTrace]
		public virtual StockPosition StockPosition {
			get => new StockPosition(Nomenclature, WearPercent, WearSize, Height, WarehouseOperation.Owner);
			set {
				Nomenclature = value.Nomenclature;
				WearSize = value.WearSize;
				Height = value.Height;
				WearPercent = value.WearPercent;
				WarehouseOperation.Owner = value.Owner;
			}
		}

		[Display(Name = "Строка дежурной нормы")]
		[IgnoreHistoryTrace]
		public virtual DutyNormItem DutyNormItem {
			get => Operation.DutyNormItem;
			set { if(Operation.DutyNormItem != value) {
					Operation.DutyNormItem = value;
					OnPropertyChanged();
				}
			}
		}
		[Display(Name = "Износ")]
		[IgnoreHistoryTrace]
		public virtual decimal WearPercent {
			get => Operation.WearPercent;
			set { if(Operation.WearPercent != value) {
					Operation.WearPercent = value;
					OnPropertyChanged();
				}
			}
		} 
		[Display(Name = "Номенклатура нормы")]
		[IgnoreHistoryTrace]
		public virtual ProtectionTools ProtectionTools {
			get => Operation.ProtectionTools;
			set { if(Operation.ProtectionTools != value) {
					Operation.ProtectionTools = value;
					OnPropertyChanged();
				}
			}
		}
		[Display (Name = "Номенклатура")]
		[IgnoreHistoryTrace]
		public virtual Nomenclature Nomenclature {
			get => Operation.Nomenclature;
			set { if(Operation.Nomenclature != value) {
					Operation.Nomenclature = value;
					OnPropertyChanged();
				}
			}
		}
		[Display (Name = "Количество")]
		[IgnoreHistoryTrace]
		public virtual int Amount {
			get => Operation.Issued;
			set { if(Operation.Issued != value) {
					Operation.Issued = value;
					OnPropertyChanged();
				}
			}
		}
		[Display(Name = "Размер")]
		[IgnoreHistoryTrace]
		public virtual Size WearSize {
			get => Operation.WearSize;
			set { if(Operation.WearSize != value) {
					Operation.WearSize = value;
					OnPropertyChanged();
				}
			}
		}
		[Display(Name = "Рост одежды")]
		[IgnoreHistoryTrace]
		public virtual Size Height {
			get => Operation.Height;
			set { if(Operation.Height != value) {
					Operation.Height = value;
					OnPropertyChanged();
				}
			}
		}
		#endregion

		#region Методы
		public virtual void UpdateOperation(IUnitOfWork uow) {
        	if (Operation == null) 
        		Operation = new DutyNormIssueOperation();

        	Operation.Update(this);
        	uow.Save(Operation);
        	
        	WarehouseOperation.Update(uow, this);
        	uow.Save(WarehouseOperation);
        }
		#endregion
	}
}
