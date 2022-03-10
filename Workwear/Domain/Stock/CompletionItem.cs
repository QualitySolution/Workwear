using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
    public class CompletionItem : PropertyChangedBase, IDomainObject
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
        public virtual int Amount {
            get => WarehouseOperation?.Amount ?? 0;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.Amount = value;
            }
        }
        [Display(Name = "Складская операция")]
        public virtual WarehouseOperation WarehouseOperation { get; set; }
        [Display(Name = "Размер")]
        public virtual string Size {
            get => WarehouseOperation?.Size;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.Size = value;
            }
        }
        [Display(Name = "Рост одежды")]
        public virtual string Growth {
            get => WarehouseOperation?.Growth;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.Growth = value;
            }
        }
        [Display(Name = "Процент износа")]
        public virtual decimal WearPercent {
            get => WarehouseOperation?.WearPercent ?? 0;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.WearPercent = value;
            }
        }
        #endregion
        #region Constructors
        public CompletionItem(){}
        #endregion
        #region Calculate
        public virtual StockPosition StockPosition => new StockPosition(Nomenclature, Size, Growth, WearPercent);
        #endregion
    }
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "строки разукомплектации",
        Nominative = "строка разукомплектации",
        Genitive = "строку разукомплектации"
    )]
    [HistoryTrace]
    public class CompletionSourceItem: CompletionItem { 
        public CompletionSourceItem(){}
        public virtual string Title { get; set; } = "строка разукомплектации";
    }

    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "строки комплектации",
        Nominative = "строка комплектации",
        Genitive = "строку комплектации"
    )]
    [HistoryTrace]
    public class CompletionResultItem : CompletionItem {
        public CompletionResultItem(){} 
        public virtual string Title { get; set; } = "строка комплектации";
    }
}