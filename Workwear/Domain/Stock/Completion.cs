using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace workwear.Domain.Stock
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "комплектации номенклатуры",
        Nominative = "комплектация номенклатуры",
        Genitive = "комплектации номенклатуры"
    )]
    [HistoryTrace]
    public class Completion: StockDocument, IValidatableObject
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
        
        #region Свойства
        private Warehouse sourceWarehouse;
        [Display(Name = "Склад списания")]
        [Required(ErrorMessage = "Склад списания должен быть указан.")]
        public virtual Warehouse SourceWarehouse {
            get { return sourceWarehouse; }
            set { SetField(ref sourceWarehouse, value, () => SourceWarehouse); }
        }
        
        private Warehouse receiptWarehouse;
        [Display(Name = "Склад получения")]
        [Required(ErrorMessage = "Склад получения должен быть указан.")]
        public virtual Warehouse ReceiptWarehouse {
            get { return receiptWarehouse; }
            set { SetField(ref receiptWarehouse, value, () => ReceiptWarehouse); }
        }
        
        private IList<CompletionSourceItem> sourceCompletionItems = new List<CompletionSourceItem>();
        [Display (Name = "расходная номенклатура")]
        public virtual IList<CompletionSourceItem> SourceItems {
            get { return sourceCompletionItems; }
            set { SetField (ref sourceCompletionItems, value, () => SourceItems); }
        }
        private System.Data.Bindings.Collections.Generic.GenericObservableList<CompletionSourceItem> observableSourceItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual System.Data.Bindings.Collections.Generic.GenericObservableList<CompletionSourceItem> ObservableSourceItems {
            get {
                if (observableSourceItems == null)
                    observableSourceItems = new System.Data.Bindings.Collections.Generic.GenericObservableList<CompletionSourceItem> (SourceItems);
                return observableSourceItems;
            }
        }
        private IList<ComplectionResultItem> resultItems = new List<ComplectionResultItem>();
        [Display (Name = "расходная номенклатура")]
        public virtual IList<ComplectionResultItem> ResultItems {
            get { return resultItems; }
            set { SetField (ref resultItems, value, () => ResultItems); }
        }
        private System.Data.Bindings.Collections.Generic.GenericObservableList<ComplectionResultItem> observableResultItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual System.Data.Bindings.Collections.Generic.GenericObservableList<ComplectionResultItem> ObservableResultItems {
            get {
                if (observableResultItems == null)
                    observableResultItems = new System.Data.Bindings.Collections.Generic.GenericObservableList<ComplectionResultItem> (ResultItems);
                return observableResultItems;
            }
        }
        #endregion

        #region Расчётные
        public virtual string Title => String.Format ("Комплектация №{0} от {1:d}" ,Id, Date);
        #endregion

        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date < new DateTime(2008, 1, 1))
                yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
                    new[] { nameof(Date)});
					
            if(SourceItems.All(i => i.Amount <= 0))
                yield return new ValidationResult ("Документ должен содержать хотя бы одну строку разукомплектации с количеством больше 0.", 
                    new[] { nameof(SourceItems)});
            
            if(SourceItems.All(i => i.Amount <= 0))
                yield return new ValidationResult ("Документ должен содержать хотя бы одну строку комплектации с количеством больше 0.", 
                    new[] { nameof(SourceItems)});
        }
        #endregion

        #region Items
        public virtual void AddSourceItem(StockPosition position, Warehouse warehouse, int count)
        {
        }
        public virtual void AddResultItem(Nomenclature nomenclature)
        {;
        }
        #endregion
    }
}