using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Sizes;

namespace workwear.Domain.Company
{
    [Appellative (Gender = GrammaticalGender.Masculine,
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
        private EmployeeCard employee;
        [Display (Name = "Сотрудник")]
        public virtual EmployeeCard Employee {
            get => employee;
            set => SetField(ref employee, value);
        }

        private Size size;
        [Display (Name = "Размер")]
        public virtual Size Size {
            get => size;
            set => SetField(ref size, value);
        }

        private SizeType sizeType;
        [Display (Name = "Тип размера")]
        public virtual SizeType SizeType {
            get => sizeType;
            set => SetField(ref sizeType, value);
        }
        #endregion
        #region Расчётные
        public virtual string Title => $"{SizeType.Name} - {Size.Name} сотрудника: {Employee.ShortName}";
        #endregion
    }
}