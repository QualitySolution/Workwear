using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using workwear.Domain.Operations;
using workwear.Domain.Sizes;

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
        [IgnoreHistoryTrace]
        public virtual WarehouseOperation WarehouseOperation { get; set; }
        [Display(Name = "Размер")]
        [Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")]
        public virtual string Size {
            get => WarehouseOperation?.Size;
            set {
                if (WarehouseOperation == null) return;
                WarehouseOperation.Size = value;
            }
        }
        [Display(Name = "Рост одежды")]
        [Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")]
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
        [Display(Name = "Размер")]
        public virtual Size WearSize { get; set; }
        [Display(Name = "Рост одежды")]
        public virtual Size Height { get; set; }
        #endregion
        #region Constructors
        public CompletionItem(){}
        #endregion
        #region Calculate
        public virtual StockPosition StockPosition => new StockPosition(Nomenclature, WearPercent, WearSize, Height);
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