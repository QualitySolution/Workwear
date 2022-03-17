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
        
        public virtual string Name { get; set; }
        
        public virtual bool UseInEmployee { get; set; }
        
        public virtual Category Category { get; set; }
        
        public virtual int Position { get; set; }
    }

    public enum Category
    {
        Size,
        Height
    }
}