using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Workwear.Domain.Visits {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "графики работы",
		Nominative = "график работы",
		Genitive = "графика работы"
	)]
	public class DaySchedule : IDomainObject {
		public virtual int Id { get; set; }

		private int dayOfWeek;
		[Display(Name = "Номер дня недели")]
		//(Пн-Вс 1-7) Используется для постоянных расписаний
		public virtual int DayOfWeak {
			get => dayOfWeek;
			set => dayOfWeek = value; 
		}
		
		private DateTime? date;
		[Display(Name = "Дата")]
		//Если указан, то это расписание для конкретного дня (исключение)
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
		[Display(Name = "Начало рабочего интервала")]
		public virtual string StartString {
			get => startString;
			set => startString = value; 
		}
		
		private string endString;
		[Display(Name = "Окончания рабочего интервала")]
		public virtual string EndString {
			get => endString;
			set => endString = value; 
		}				

		public virtual TimeSpan Start => String.IsNullOrEmpty(startString) ? TimeSpan.Zero : TimeSpan.Parse(startString);
		public virtual TimeSpan End => String.IsNullOrEmpty(endString) ? TimeSpan.Zero : TimeSpan.Parse(endString);

		public virtual bool IsWork => StartString != null && EndString != null && interval != 0;
		public virtual string Title {
			get {
				if(IsWork) {
					string intervalInfo = $"{Start:hh\\:mm} - {End:hh\\:mm} (интервал {Interval} мин.)";
					if(Date.HasValue)
						return $"{Date.Value:dd.MM.yyyy}: {intervalInfo}";
					else
						return $"День недели {DayOfWeak}: {intervalInfo}";
				}
				else {
					if(Date.HasValue)
						return $"{Date.Value:dd.MM.yyyy}: Выходной";
					else
						return $"День недели {DayOfWeak}: Выходной";
				}
			}
		}
	}
}
