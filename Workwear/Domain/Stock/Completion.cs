using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using workwear.Domain.Operations;
using BindingsObservableSourceList =  System.Data.Bindings.Collections.Generic.GenericObservableList<workwear.Domain.Stock.CompletionSourceItem>;
using BindingsObservableResultList =  System.Data.Bindings.Collections.Generic.GenericObservableList<workwear.Domain.Stock.CompletionResultItem>;

namespace workwear.Domain.Stock
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "комплектации номенклатуры",
        Nominative = "комплектация номенклатуры",
        Genitive = "комплектацию номенклатуры")]
    [HistoryTrace]
    public class Completion: StockDocument, IValidatableObject
    {
        #region Свойства
        private Warehouse sourceWarehouse;
        [Display(Name = "Склад разукомплектации")]
        [Required(ErrorMessage = "Склад разукомплектации должен быть указан")]
        public virtual Warehouse SourceWarehouse {
            get => sourceWarehouse;
            set => SetField(ref sourceWarehouse, value);
        }
        private Warehouse resultWarehouse;
        [Display(Name = "Склад комплектации")]
        [Required(ErrorMessage = "Склад комплектации должен быть указан")]
        public virtual Warehouse ResultWarehouse {
            get => resultWarehouse;
            set => SetField(ref resultWarehouse, value);
        }
        private IList<CompletionSourceItem> sourceCompletionItems = new List<CompletionSourceItem>();
        [Display (Name = "позиция разукомплектации")]
        public virtual IList<CompletionSourceItem> SourceItems {
            get => sourceCompletionItems;
            set => SetField (ref sourceCompletionItems, value);
        }
        private BindingsObservableSourceList observableSourceItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual BindingsObservableSourceList ObservableSourceItems => 
            observableSourceItems ?? (observableSourceItems = new BindingsObservableSourceList(SourceItems));
        private IList<CompletionResultItem> resultItems = new List<CompletionResultItem>();
        [Display (Name = "позиция комплектации")]
        public virtual IList<CompletionResultItem> ResultItems {
            get => resultItems;
            set => SetField (ref resultItems, value);
        }
        private BindingsObservableResultList observableResultItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual BindingsObservableResultList ObservableResultItems => 
            observableResultItems ?? (observableResultItems = new BindingsObservableResultList(ResultItems));
        #endregion
        #region Расчётные
        public virtual string Title => String.Format("Комплектация №{0} от {1:d}" ,Id, Date);
        #endregion
        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date < new DateTime(2008, 1, 1))
                yield return new ValidationResult("Дата должны указана (не ранее 2008-го)",
                    new[] {nameof(Date)});
            if (SourceWarehouse is null)
                yield return new ValidationResult("Склад разукомплектации должен быть указан",
                    new[] {nameof(SourceWarehouse)});
            if (ResultWarehouse is null)
                yield return new ValidationResult("Склад комплектации должен быть указан",
                    new[] {nameof(ResultWarehouse)});
            foreach (var item in SourceItems) {
                if (item.Amount == 0)
                    yield return new ValidationResult(
                        $"{item.StockPosition.Title}: строка разукомплектации должна содержать кол-во",
                        new[] {nameof(SourceItems)});
            }
            foreach (var item in ResultItems) {
                if (item.Amount == 0)
                    yield return new ValidationResult(
                        $"{item.StockPosition.Title}: строка комплектации должна содержать кол-во",
                        new[] {nameof(SourceItems)});
            }
        }
        #endregion
        #region Items
        public virtual void AddSourceItem(StockPosition position, Warehouse warehouse, int count) {
            if (SourceWarehouse is null) SourceWarehouse = warehouse;
            var item = new CompletionSourceItem {
                Completion = this,
                WarehouseOperation = new WarehouseOperation() {
                    Nomenclature = position.Nomenclature,
                    OperationTime = Date,
                    ExpenseWarehouse = SourceWarehouse,
                    Size = position.Size,
                    Growth = position.Growth,
                    Amount = count,
                    WearPercent = position.WearPercent
                }
            };
            ObservableSourceItems.Add(item);
        }
        public virtual void AddResultItem(Nomenclature nomenclature) {
            var item = new CompletionResultItem {
                Completion = this,
                WarehouseOperation = new WarehouseOperation() {
                    Nomenclature = nomenclature,
                    OperationTime = Date,
                    ReceiptWarehouse = ResultWarehouse
                }
            };
            ObservableResultItems.Add(item);
        }
        public virtual void UpdateItems() {
            foreach (var item in SourceItems) {
                item.WarehouseOperation.ExpenseWarehouse = SourceWarehouse;
                item.WarehouseOperation.OperationTime = Date;
            }
            foreach (var item in ResultItems) {
                item.WarehouseOperation.ReceiptWarehouse = ResultWarehouse;
                item.WarehouseOperation.OperationTime = Date;
            }
        }
        #endregion
    }
}