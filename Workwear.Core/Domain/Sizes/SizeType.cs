using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace workwear.Domain.Sizes
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "типы размеров",
        Nominative = "тип размеров",
        Genitive = "типа размеров"
    )]
    [HistoryTrace]
    public class SizeType: PropertyChangedBase, IDomainObject
    {
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
    }

    public enum Category
    {
        [Display(Name = "Размер")]
        Size,
        [Display(Name = "Рост")]
        Height
    }
}