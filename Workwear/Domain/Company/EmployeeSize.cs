using QS.DomainModel.Entity;
using QS.HistoryLog;
using workwear.Domain.Sizes;

namespace workwear.Domain.Company
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "размеры сотрудников",
        Nominative = "размер сотрудника",
        PrepositionalPlural = "размерах сотрудников",
        Genitive = "размера сотрудника"
    )]
    [HistoryTrace]
    public class EmployeeSize: PropertyChangedBase, IDomainObject
    {
        #region Свойства
        public virtual int Id { get; }
        public virtual EmployeeCard Employee { get; set; }
        public virtual Size Size { get; set; }
        
        private SizeType sizeType;
        public virtual SizeType SizeType {
            get => sizeType;
            set => SetField(ref sizeType, value);
        }
        #endregion
        #region Расчётные
        public virtual string Title => $"{SizeType.Name} - {Size.Name} сотрудника: {Employee.Title}";
        #endregion
    }
}