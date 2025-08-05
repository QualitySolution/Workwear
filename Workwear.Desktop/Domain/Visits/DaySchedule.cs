using System;
using QS.DomainModel.Entity;

namespace Workwear.Domain.Visits {
	public class DaySchedule : IDomainObject {
		public virtual int Id { get; set; }

		private int dayOfWeek;
		//В отличии от System.DayOfWeak Восскресенье обозначается как 7
		public virtual int DayOfWeak {
			get => dayOfWeek;
			set => dayOfWeek = value; 
		}

		private int interval;
		public virtual int Inteval {
			get => interval;
			set => interval = value; 
		}
		
		private string startString;
		public virtual string StartString {
			get => startString;
			set => startString = value; 
		}
		
		private string endString;
		public virtual string EndString {
			get => endString;
			set => endString = value; 
		}

		public virtual TimeSpan Start => TimeSpan.Parse(startString);
		public virtual TimeSpan End => TimeSpan.Parse(endString);
	}
}
