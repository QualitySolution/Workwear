using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using BindingsObservableSourceList =  System.Data.Bindings.Collections.Generic.GenericObservableList<Workwear.Domain.Stock.Documents.CompletionSourceItem>;
using BindingsObservableResultList =  System.Data.Bindings.Collections.Generic.GenericObservableList<Workwear.Domain.Stock.Documents.CompletionResultItem>;

namespace Workwear.Domain.Stock.Documents
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "комплектации номенклатуры",
        Nominative = "комплектация номенклатуры",
        Genitive = "комплектации номенклатуры")]
    [HistoryTrace]
    public class Completion: StockDocument, IValidatableObject
    {
        #region Свойства
        private Warehouse sourceWarehouse;
        [Display(Name = "Склад комплектующих")]
        public virtual Warehouse SourceWarehouse {
            get => sourceWarehouse;
            set => SetField(ref sourceWarehouse, value);
        }
        private Warehouse resultWarehouse;
        [Display(Name = "Склад получения результата")]
        public virtual Warehouse ResultWarehouse {
            get => resultWarehouse;
            set => SetField(ref resultWarehouse, value);
        }
        private IList<CompletionSourceItem> sourceCompletionItems = new List<CompletionSourceItem>();
        [Display (Name = "Комплектующие")]
        public virtual IList<CompletionSourceItem> SourceItems {
            get => sourceCompletionItems;
            set => SetField (ref sourceCompletionItems, value);
        }
        private BindingsObservableSourceList observableSourceItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual BindingsObservableSourceList ObservableSourceItems => 
            observableSourceItems ?? (observableSourceItems = new BindingsObservableSourceList(SourceItems));
        private IList<CompletionResultItem> resultItems = new List<CompletionResultItem>();
        [Display (Name = "Результат комплектации")]
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
                yield return new ValidationResult("Склад комплектующих должен быть указан",
                    new[] {nameof(SourceWarehouse)});
            if (ResultWarehouse is null)
                yield return new ValidationResult("Склад получения результата должен быть указан",
                    new[] {nameof(ResultWarehouse)});
            if (SourceItems.Count == 0)
                yield return new ValidationResult("Не указаны комплектующие",
                    new[] {nameof(SourceWarehouse)});
            if (ResultItems.Count == 0)
                yield return new ValidationResult("Результат комплектации должен содержать хотя бы одну позицию",
                    new[] {nameof(ResultWarehouse)});
            foreach (var item in SourceItems) {
                if (item.Amount == 0)
                    yield return new ValidationResult(
                        $"{item.StockPosition.Title}: строка комплектующих должна содержать кол-во",
                        new[] {nameof(SourceItems)});
            }
            foreach (var item in ResultItems) {
                if (item.Amount == 0)
                    yield return new ValidationResult(
                        $"{item.StockPosition.Title}: строка результата комплектации должна содержать кол-во",
                        new[] {nameof(SourceItems)});
            }
            var baseParameters = (BaseParameters) validationContext.Items[nameof(BaseParameters)];
            if (!baseParameters.CheckBalances) yield break; {
                var uow = (IUnitOfWork) validationContext.Items[nameof(IUnitOfWork)];
                var repository = new StockRepository();
                foreach (var item in SourceItems) {
                    var nomenclatures = new List<Nomenclature> {item.Nomenclature};
                    var balance = repository
                        .StockBalances(uow, SourceWarehouse, nomenclatures, DateTime.Now, new List<WarehouseOperation> {item.WarehouseOperation})
                        .Where(s => Equals(s.StockPosition, item.StockPosition))
                        .ToList();
                    if (!balance.Any()) {yield return new ValidationResult(
                            $"Для комплектации не хватает комплектующих на складе" +
                            $" склад {SourceWarehouse?.Name} не содержит содержит {item.StockPosition.Title}");
                    }
                    else 
                        foreach (var balanceItem in balance.Where(balanceItem => balanceItem.Amount < item.Amount)) {
                            yield return new ValidationResult(
                                $"Для комплектации {item.StockPosition.Title} не хватает комплектующих на складе" +
                                $" склад {SourceWarehouse?.Name} содержит {balanceItem.Amount} {item.Nomenclature.Type.Units.Name}");
                        }
                }
            }
        }

        #endregion
        #region Items
        public virtual void AddSourceItem(StockPosition position, Warehouse warehouse, int count) {
            if (SourceWarehouse is null) SourceWarehouse = warehouse;
            var item = new CompletionSourceItem {
                Completion = this,
                WarehouseOperation = new WarehouseOperation {
                    Nomenclature = position.Nomenclature,
                    OperationTime = Date,
                    ExpenseWarehouse = SourceWarehouse,
                    WearSize = position.WearSize,
                    Height = position.Height,
                    Amount = count,
                    WearPercent = position.WearPercent,
                    Owner = position.Owner
                }
            };
            ObservableSourceItems.Add(item);
        }
        public virtual CompletionResultItem AddResultItem(Nomenclature nomenclature) {
            var item = new CompletionResultItem {
                Completion = this,
                WarehouseOperation = new WarehouseOperation
                {
                    Nomenclature = nomenclature,
                    OperationTime = Date,
                    ReceiptWarehouse = ResultWarehouse
                }
            };
            ObservableResultItems.Add(item);
            return item;
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
