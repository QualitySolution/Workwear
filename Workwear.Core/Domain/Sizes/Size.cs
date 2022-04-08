using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Measurements;

namespace workwear.Domain.Sizes
{
    [Appellative (Gender = GrammaticalGender.Masculine,
        NominativePlural = "размеры",
        Nominative = "размер",
        Genitive = "размера"
    )]
    [HistoryTrace]
    public class Size: PropertyChangedBase, IDomainObject, IValidatableObject
    {
        #region Свойства
        public virtual int Id { get; }
        private string name;
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
        private SizeType sizeType;
        public virtual SizeType SizeType {
            get => sizeType;
            set => sizeType = value;
        }
        private bool useInEmployee;
        public virtual bool UseInEmployee {
            get => useInEmployee;
            set => useInEmployee = value;
        }
        private bool useInNomenclature;
        public virtual bool UseInNomenclature {
            get => useInNomenclature;
            set => useInNomenclature = value;
        }
        #endregion
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
        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (SizeType is null)
                yield return new ValidationResult (
                    "Тип размера должен быть указан", 
                    new[] { this.GetPropertyName(s => s.Title)});
            if (Name is null)
                yield return new ValidationResult (
                    "Значение должно быть указано", 
                    new[] { this.GetPropertyName(s => s.Title)});
            if(SuitableSizes.Contains(this))
                yield return new ValidationResult (
                    "Размер не может быть своим аналогом", 
                    new[] { this.GetPropertyName(s => s.Title)});
            var uow = (IUnitOfWork) validationContext.Items[nameof(IUnitOfWork)];
            var doubleSize = 
                    SizeService
                        .GetSize(uow, SizeType)
                        .FirstOrDefault(x => x.Name == Name && x.Id != Id);
            if(doubleSize != null)
                yield return new ValidationResult (
                    "Такой размер уже существует", 
                    new[] { this.GetPropertyName(s => s.Title)});
        }
        #endregion
    }
}