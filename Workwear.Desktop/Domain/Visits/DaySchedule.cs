using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Workwear.Domain.Visits {
	public class DaySchedule : IDomainObject {
		public virtual int Id { get; set; }

		private int dayOfWeek;
		[Display(Name = "Номер дня недели")]
		//(Пн-Вс 1-7) Используется для постоянных рассписаний
		public virtual int DayOfWeak {
			get => dayOfWeek;
			set => dayOfWeek = value; 
		}
		
		private DateTime? date;
		[Display(Name = "Дата")]
		//Если указан, то это рассписание для конкретного дня (исключение)
		public virtual DateTime? Date {
			get => date?.Date;
			set => date = value; 
		}

		private int interval;
		[Display(Name = "Интервал в минутах")]
		public virtual int Interval {
			get => interval;
			set => interval = value; 
		}
		
		private string startString;
		[Display(Name = "Время начала рабочего дня")]
		public virtual string StartString {
			get => startString;
			set => startString = value; 
		}
		
		private string endString;
		[Display(Name = "Время окончания рабочего дня")]
		public virtual string EndString {
			get => endString;
			set => endString = value; 
		}				

		public virtual TimeSpan Start => String.IsNullOrEmpty(startString) ? TimeSpan.Zero : TimeSpan.Parse(startString);
		public virtual TimeSpan End => String.IsNullOrEmpty(endString) ? TimeSpan.Zero : TimeSpan.Parse(endString);

		public virtual bool IsWork => StartString != null && EndString != null && interval != 0;
	}
}
