using QS.DomainModel.Entity;
using workwear.Domain.Sizes;

namespace workwear.Domain.Company
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "размеры сотрудников",
        Nominative = "размер сотрудника",
        PrepositionalPlural = "размерах сотрудников",
        Genitive = "размера сотрудника"
    )]
    public class EmployeeSize: PropertyChangedBase, IDomainObject
    {
        #region Свойства
        public virtual int Id { get; }
        public virtual EmployeeCard Employee { get; set; }
        public virtual Size Size { get; set; }
        public virtual SizeType SizeType { get; set; }
        #endregion
        #region Расчётные
        public virtual string Title => $"{SizeType.Name} - ${Size.Name} сотрудника: ${Employee.Title} №{Id}";
        #endregion
    }
}