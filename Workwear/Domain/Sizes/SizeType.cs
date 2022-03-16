using QS.DomainModel.Entity;

namespace workwear.Domain.Sizes
{
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