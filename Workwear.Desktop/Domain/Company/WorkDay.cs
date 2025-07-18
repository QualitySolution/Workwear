using System;
using QS.DomainModel.Entity;

namespace Workwear.Domain.Company {
		[Appellative(Gender = GrammaticalGender.Masculine,
    		NominativePlural = "рабочие дни",
    		Nominative = "рабочий день",
    		Genitive = "дабочего дня",
    		GenitivePlural = "рабочих дней"
    		)]
    	public class WorkDay : IDomainObject{
		public virtual int Id { get; }

		private DateTime date;
		public virtual DateTime Date {
			get => date.Date;
			set => date = value; 
		}

		public virtual bool IsWorkday { get; set; }
		public virtual string Comment { get; set; }
	    }
}
