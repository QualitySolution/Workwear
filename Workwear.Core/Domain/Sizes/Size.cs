using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace workwear.Domain.Sizes
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "размеры",
        Nominative = "размер",
        Genitive = "размера"
    )]
    [HistoryTrace]
    public class Size: PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; }
        public virtual string Name { get; set; }
        public virtual SizeType SizeType { get; set; }
        public virtual bool UseInEmployee { get; set; }
        public virtual bool UseInNomenclature { get; set; }

        #region Suitable
        private IList<Size> suitableSizes  = new List<Size>();
        [Display(Name = "Подходящие размеры")]
        public virtual IList<Size> SuitableSizes {
            get => suitableSizes;
            set => SetField(ref suitableSizes, value);
        }
        private GenericObservableList<Size> observableSuitableSizes;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<Size> ObservableSuitableSizes => 
            observableSuitableSizes ?? (observableSuitableSizes = new GenericObservableList<Size>(SuitableSizes));
        #endregion

        #region Расчётные
        public virtual string Title => $"{SizeType.Name}: {Name}";
        #endregion
    }
}