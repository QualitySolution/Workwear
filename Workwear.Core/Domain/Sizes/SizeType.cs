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
    [Appellative (Gender = GrammaticalGender.Feminine,
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
        public virtual bool UseInEmployee { get; set; }

        private Category category;
        public virtual Category Category {
            get => category;
            set => SetField(ref category, value);
        }
        public virtual int Position { get; set; }
        #endregion
        
        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(Position == 0)
                yield return new ValidationResult (
                "Позиция должна быть больше нуля", 
                new[] { this.GetPropertyName(s => s.Name)});
            if (Name is null)
                yield return new ValidationResult (
                    "Имя должно быть указано", 
                    new[] { this.GetPropertyName(s => s.Name)});
            var uow = (IUnitOfWork) validationContext.Items[nameof(IUnitOfWork)];
            var dubblePos = SizeService.GetSizeType(uow).FirstOrDefault(x => x.Position == Position && x.Id != Id);
            if(dubblePos != null)
                yield return new ValidationResult (
                    $"Позиция:{Position} уже занята", 
                    new[] { dubblePos.GetPropertyName(s => s.Name)});
            var dubbleName = SizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == Name && x.Id != Id);
            if(dubbleName != null)
                yield return new ValidationResult (
                    $"Имя:{Name} уже занята", 
                    new[] { dubbleName.GetPropertyName(s => s.Name)});
        }
        #endregion
    }

    public enum Category
    {
        [Display(Name = "Размер")]
        Size,
        [Display(Name = "Рост")]
        Height
    }
}