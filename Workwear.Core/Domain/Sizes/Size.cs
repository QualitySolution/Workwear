using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Measurements;

#if DESKTOP
using System.Data.Bindings.Collections.Generic;
#endif

namespace Workwear.Domain.Sizes
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
        [Display (Name = "Значение")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
        private SizeType sizeType;
        [Display (Name = "Тип размера")]
        public virtual SizeType SizeType {
            get => sizeType;
            set => SetField(ref sizeType, value);
        }
        private bool useInEmployee;
        [Display (Name = "Отображается в сотруднике")]
        public virtual bool UseInEmployee {
            get => useInEmployee;
            set => SetField(ref useInEmployee, value);
        }
        private bool useInNomenclature;
        [Display (Name = "Отображается в номенклатуре")]
        public virtual bool UseInNomenclature {
            get => useInNomenclature;
            set => SetField(ref useInNomenclature, value);
        }
        private string alternativeName;
        [Display(Name = "Альтернативное значение")]
        public virtual string AlternativeName {
            get => alternativeName;
            set => SetField(ref alternativeName, value);
        }
        #endregion
        #region Suitable
        private IList<Size> suitableSizes  = new List<Size>();
        [Display(Name = "Подходящие размеры")]
        public virtual IList<Size> SuitableSizes {
            get => suitableSizes;
            set => SetField(ref suitableSizes, value);
        }
#if DESKTOP
        private GenericObservableList<Size> observableSuitableSizes;
        //FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        [Display(Name = "Подходящие размеры")]
        public virtual GenericObservableList<Size> ObservableSuitableSizes => 
            observableSuitableSizes ?? (observableSuitableSizes = new GenericObservableList<Size>(SuitableSizes));
#endif
        #endregion
        #region Расчётные
        public virtual string Title => $"{SizeType.Name}: {Name}";
        #endregion
        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (SizeType is null)
                yield return new ValidationResult ("Тип размера должен быть указан");
            if (String.IsNullOrEmpty(Name))
                yield return new ValidationResult ("Значение должно быть указано");
            if(SuitableSizes.Contains(this))
                yield return new ValidationResult ("Размер не может быть своим аналогом");
            var uow = (IUnitOfWork) validationContext.Items[nameof(IUnitOfWork)];
            var sizeService = new SizeService();
            var doubleSize = 
                    sizeService
                        .GetSize(uow, SizeType)
                        .FirstOrDefault(x => x.Name == Name && x.Id != Id);
            if(doubleSize != null)
                yield return new ValidationResult ("Такой размер уже существует");
        }
        #endregion
    }
}