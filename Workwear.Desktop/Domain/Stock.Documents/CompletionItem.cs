using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents
{
    public class CompletionItem : PropertyChangedBase, IDomainObject, IDocItemSizeInfo
    {
        #region Свойства
        public virtual int Id { get; }
        [IgnoreHistoryTrace]
        public virtual Completion Completion { get; set; }
        public virtual Nomenclature Nomenclature {
            get => WarehouseOperation?.Nomenclature;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.Nomenclature = value;
            }
        }
        [Display(Name = "Кол-во")]
        [PropertyChangedAlso(nameof(WarehouseOperation))]
        public virtual int Amount {
            get => WarehouseOperation?.Amount ?? 0;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.Amount = value;
                OnPropertyChanged();
            }
        }
        private WarehouseOperation warehouseOperation;
        [Display(Name = "Складская операция")]
        [IgnoreHistoryTrace]
        public virtual WarehouseOperation WarehouseOperation {
            get => warehouseOperation;
            set => SetField(ref warehouseOperation, value);
        }
        [Display(Name = "Процент износа")]
        public virtual decimal WearPercent {
            get => WarehouseOperation?.WearPercent ?? 0;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.WearPercent = value;
            }
        }
        [Display(Name = "Размер")]
        public virtual Size WearSize {
            get => WarehouseOperation.WearSize;
            set =>  WarehouseOperation.WearSize = value;
        }
        [Display(Name = "Рост одежды")]
        public virtual Size Height {
            get => WarehouseOperation.Height;
            set => WarehouseOperation.Height = value;
        }
        [Display(Name = "Тип Роста")]
        public virtual SizeType HeightType {
	        get => warehouseOperation.Nomenclature.Type.HeightType;
        }
        [Display(Name = "Тип размера одежды")]
        public virtual SizeType WearSizeType {
	        get => warehouseOperation.Nomenclature.Type.SizeType;
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

        #endregion
        #region Constructors
        public CompletionItem(){}
        #endregion
        #region Calculate
        public virtual StockPosition StockPosition => 
            new StockPosition(Nomenclature, WearPercent, WarehouseOperation.WearSize, WarehouseOperation.Height, WarehouseOperation.Owner);
        #endregion
    }
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "строки разукомплектации",
        Nominative = "строка разукомплектации",
        Genitive = "строку разукомплектации"
    )]
    [HistoryTrace]
    public class CompletionSourceItem: CompletionItem {
        public CompletionSourceItem() {}
        public virtual string Title => $"Комплектующий {StockPosition.Title} в документе {Completion.Title}";
    }

    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "строки комплектации",
        Nominative = "строка комплектации",
        Genitive = "строку комплектации"
    )]
    [HistoryTrace]
    public class CompletionResultItem : CompletionItem {
        public CompletionResultItem() {}
        public virtual string Title => $"Результат комплектации {StockPosition.Title} в документе {Completion.Title}";
    }
}
