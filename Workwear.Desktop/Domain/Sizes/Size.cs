using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Tools.Sizes;

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
        public virtual int Id { get; set; }
        private string name;
        [Display (Name = "Значение")]
        [StringLength(10, ErrorMessage = "Максимальный размер значения 10 символов")]
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
        private bool showInEmployee;
        [Display (Name = "Отображается в сотруднике")]
        public virtual bool ShowInEmployee {
            get => showInEmployee;
            set => SetField(ref showInEmployee, value);
        }
        private bool showInNomenclature;
        [Display (Name = "Отображается в номенклатуре")]
        public virtual bool ShowInNomenclature {
            get => showInNomenclature;
            set => SetField(ref showInNomenclature, value);
        }
        private string alternativeName;
        [Display(Name = "Альтернативное значение")]
        [StringLength(10, ErrorMessage = "Максимальный размер альтернативного значения 10 символов")]
        public virtual string AlternativeName {
            get => alternativeName;
            set => SetField(ref alternativeName, value);
        }
        #endregion
        #region Suitable
        private IObservableList<Size> suitableSizes  = new ObservableList<Size>();
        [Display(Name = "Подходящие размеры")]
        public virtual IObservableList<Size> SuitableSizes {
            get => suitableSizes;
            set => SetField(ref suitableSizes, value);
        }
        
        [Display(Name = "Размеры в которых этот размер указан как подходящий")]
        public virtual IList<Size> SizesWhereIsThisSizeAsSuitable { get; } = new List<Size>();

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
