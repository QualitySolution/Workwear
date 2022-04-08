using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Measurements;

namespace workwear.Domain.Sizes
{
    [Appellative (Gender = GrammaticalGender.Masculine,
        NominativePlural = "типы размеров",
        Nominative = "тип размеров",
        Genitive = "типа размеров"
    )]
    [HistoryTrace]
    public class SizeType: PropertyChangedBase, IDomainObject, IValidatableObject
    {
        #region Свойства
        public virtual int Id { get; }
        private string name;
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
        private bool useInEmployee;
        public virtual bool UseInEmployee {
            get => useInEmployee;
            set => SetField(ref useInEmployee, value);
        }
        private Category category;
        public virtual Category Category {
            get => category;
            set => SetField(ref category, value);
        }
        private int position;
        public virtual int Position {
            get => position;
            set => SetField(ref position, value);
        }
        #endregion
        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if(Position == 0)
                yield return new ValidationResult (
                "Позиция должна быть больше нуля", 
                new[] { this.GetPropertyName(s => s.Name)});
            if (Name is null)
                yield return new ValidationResult (
                    "Имя должно быть указано", 
                    new[] { this.GetPropertyName(s => s.Name)});
            var uow = (IUnitOfWork) validationContext.Items[nameof(IUnitOfWork)];
            var doublePos = 
                SizeService.GetSizeType(uow)
                    .FirstOrDefault(x => x.Position == Position && x.Id != Id);
            if(doublePos != null)
                yield return new ValidationResult (
                    $"Позиция:{Position} уже занята", 
                    new[] { doublePos.GetPropertyName(s => s.Name)});
            var doubleName = 
                SizeService.GetSizeType(uow)
                    .FirstOrDefault(x => x.Name == Name && x.Id != Id);
            if(doubleName != null)
                yield return new ValidationResult (
                    $"Имя:{Name} уже занята", 
                    new[] { doubleName.GetPropertyName(s => s.Name)});
        }
        #endregion
    }
    public enum Category {
        [Display(Name = "Размер")]
        Size,
        [Display(Name = "Рост")]
        Height
    }
}