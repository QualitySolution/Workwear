using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Measurements;

namespace Workwear.Domain.Sizes
{
    [Appellative (Gender = GrammaticalGender.Masculine,
        NominativePlural = "типы размеров",
        Nominative = "тип размеров",
        Genitive = "типа размеров"
    )]
#if DESKTOP
    [QS.HistoryLog.HistoryTrace]
#endif
    public class SizeType: PropertyChangedBase, IDomainObject, IValidatableObject
    {
        #region Свойства
        public virtual int Id { get; }
        private string name;
        [Display(Name = "Название")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
        private bool useInEmployee;
        [Display(Name = "Отображается в сотруднике")]
        public virtual bool UseInEmployee {
            get => useInEmployee;
            set => SetField(ref useInEmployee, value);
        }
        private CategorySizeType categorySizeType;
        [Display(Name = "Категория")]
        public virtual CategorySizeType CategorySizeType {
            get => categorySizeType;
            set => SetField(ref categorySizeType, value);
        }
        private int position;
        [Display(Name = "Позиция в карточке сотрудника")]
        public virtual int Position {
            get => position;
            set => SetField(ref position, value);
        }
        #endregion
        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if(Position <= 0)
                yield return new ValidationResult ("Позиция должна быть больше нуля");
            if (String.IsNullOrEmpty(Name))
                yield return new ValidationResult ("Имя должно быть указано");
            var uow = (IUnitOfWork) validationContext.Items[nameof(IUnitOfWork)];
            var sizeService = new SizeService();
            var doublePos = 
                sizeService.GetSizeType(uow)
                    .FirstOrDefault(x => x.Position == Position && x.Id != Id);
            if(doublePos != null)
                yield return new ValidationResult ($"Позиция:{Position} уже занята");
            var doubleName = 
                sizeService.GetSizeType(uow)
                    .FirstOrDefault(x => x.Name == Name && x.Id != Id);
            if(doubleName != null)
                yield return new ValidationResult ($"Имя:{Name} уже занято");
        }
        #endregion
    }
    public enum CategorySizeType {
        [Display(Name = "Размер")]
        Size,
        [Display(Name = "Рост")]
        Height
    }
}