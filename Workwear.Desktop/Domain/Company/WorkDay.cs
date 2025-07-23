using System;
using QS.DomainModel.Entity;

namespace Workwear.Domain.Company {
	[Appellative(
		Gender = GrammaticalGender.Masculine,
    	NominativePlural = "рабочие дни",
    	Nominative = "рабочий день",
    	Genitive = "рабочего дня",
    	GenitivePlural = "рабочих дней"
    	)]
    public class WorkDay : IDomainObject{
	    #region Свойства
		public virtual int Id { get; }

		private DateTime date;
		public virtual DateTime Date {
			get => date.Date;
			set => date = value; 
		}

		public virtual bool IsWorkday { get; set; }
		public virtual string Comment { get; set; }
	    #endregion

	    #region Расчётные свойства
	    public virtual string Title => $"{Date:dd.MM.yyyy} - {(IsWorkday ? "Рабочий" : "Выходной")} день";
	    #endregion
    }
}
