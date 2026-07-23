using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Visits;

namespace Workwear.Models.Visits {
	public class VisitListModel {

		public VisitListModel(List<DaySchedule> allDaysSchedule) {
			DaysSchedule = allDaysSchedule.Where(d => d.Date == null).ToList();
			ExclusiveDays = allDaysSchedule.Where(d => d.Date != null).ToList();
		}
		
		public List<DaySchedule> ExclusiveDays { get; }
		public List<DaySchedule> DaysSchedule { get;}
		//Коллекция отображаемых номерков
		public SortedDictionary<DateTime, VisitListItem> Items { get; set; } = new SortedDictionary<DateTime, VisitListItem>();

		/// <summary>
		/// </summary>
		/// <param name="day"></param>
		/// True, если на этот день есть персональное расписание или для этого дня недели оно задано.
		/// False, если на этот день недели нет расписания или эта дата явно указана выходной.
		/// <returns></returns>
		public bool IsWorkDay(DateTime day) {
			if(ExclusiveDays.Any(d => d.Date == day))
				return ExclusiveDays.Any(d => d.Date == day && d.IsWork);
			
			return DaysSchedule.Any(d => d.DayOfWeek % 7 == (int)day.DayOfWeek && d.IsWork);
		}

		/// <summary>
		/// Добавить в список записи посещений
		/// </summary>
		public void PutVisits (List<Visit> visits) {
			foreach(var visit in visits) {
				while(Items.ContainsKey(visit.VisitTime))
					visit.VisitTime = visit.VisitTime.AddSeconds(1);
				Items.Add(visit.VisitTime, new VisitListItem(visit));
			}
		}
		
		/// <summary>
		/// Сгенерировать в списке недостающие интервалы в соответствии с расписанием для этого дня
		/// </summary>
		public void FillScheduleOfDay (DateTime day) {
			foreach(var time in MakeSchedule(day).Where(x => !Items.ContainsKey(x)))
				Items.Add(time, new VisitListItem(time));
		}
		
		/// <summary>
		/// Создаёт график. Список DataTime начла возможных записей.
		/// </summary>
		private List<DateTime> MakeSchedule(DateTime day) {
			List<DateTime> result = new List<DateTime>();
			List<DaySchedule> scheduleList = ExclusiveDays.Where(d => d.Date == day).ToList();
			if(!scheduleList.Any())
				scheduleList = DaysSchedule.Where(d => d.DayOfWeek == (int)day.DayOfWeek % 7).ToList();
			foreach(var schedule in scheduleList) {
				DateTime start = day.Date + (schedule.Start); 
				DateTime end = day.Date + (schedule.End);
				for(DateTime time = start; time < end; time = time.AddMinutes(schedule.Interval.Value)) 
					result.Add(time);
			}
			return result;
		}
	}
}
