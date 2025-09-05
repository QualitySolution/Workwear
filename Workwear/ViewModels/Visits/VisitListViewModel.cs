using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Visits {
	public class VisitListViewModel : UowDialogViewModelBase 
	{
		public VisitListViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEntityChangeWatcher changeWatcher,
			INavigationManager navigation,
			IValidator validator = null,
            UnitOfWorkProvider unitOfWorkProvider = null,
			string UoWTitle = null
			) : base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.changeWatcher = changeWatcher ?? throw new ArgumentNullException(nameof(changeWatcher));
			
			Title = "Посещения склада";
			
			var allDaysSchedule = UoW.GetAll<DaySchedule>().ToList();
			DaysSchedule = allDaysSchedule.Where(d => d.Date == null).ToList();
			ExclusiveDays = allDaysSchedule.Where(d => d.Date != null).ToList();
			
			//Для обновления вюшки при действиях с документами
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Expense>();
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Return>();
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Writeoff>();

			loadData();
			if(Items.Count == 0)
				NextDay();
		}
		//TODO докумментация IDialogDocumentation
		//public string DocumentationUrl { get; }
		//public string ButtonTooltip { get; }
		
		private readonly INavigationManager navigation;
		private readonly IEntityChangeWatcher changeWatcher;
		public DateTime Day { get; set; } = DateTime.Now.Date;
		public string PeriodString => Day.Date.ToShortDateString();
		public List<DaySchedule> ExclusiveDays { get; }
		public List<DaySchedule> DaysSchedule { get;}
		//Коллекция отображаемых номерков
		public SortedDictionary<DateTime, VisitListWidgetItem> Items { get; set; } = new SortedDictionary<DateTime, VisitListWidgetItem>();

		//Для синхронизации с изменениями внесёнными в базу при открытом диалоге.
		private void VisitChangeEvent(EntityChangeEvent[] changeevents) {
			foreach(var changeEvent in changeevents)
				if(changeEvent.EventType != TypeOfChangeEvent.Insert) {
					var docExp = UoW.GetById<Expense>(changeEvent.Entity.GetId());
					if (docExp != null)
						UoW.Session.Evict(docExp);
					var docWO = UoW.GetById<Writeoff>(changeEvent.Entity.GetId());
					if (docWO != null)
						UoW.Session.Evict(docWO);
					var docRet = UoW.GetById<Return>(changeEvent.Entity.GetId());
					if (docRet != null)
						UoW.Session.Evict(docRet);
				}
			loadData();
		}
	
		private void loadData() {

			//Очистка отображаемой коллекции
			foreach(var v in Items.Where(x => x.Value.Visit != null)) 
				UoW.Session.Evict(v.Value.Visit);
			Items.Clear();
			
			var visits = UoW.GetAll<Visit>() 
				.Where(x => x.VisitTime >= Day)
				.Where(x => x.VisitTime < Day.AddDays(1))
				.Fetch(x => x.Employee)
				.ToList();
			
			foreach(var visit in visits) {
				while(Items.ContainsKey(visit.VisitTime))
					visit.VisitTime = visit.VisitTime.AddSeconds(1);
				Items.Add(visit.VisitTime, new VisitListWidgetItem(visit));
			}

			if(IsWorkDay(Day))
				foreach(var time in MakeSchedule(Day).Where(x => !Items.ContainsKey(x)))
					Items.Add(time, new VisitListWidgetItem(time));
			
			OnPropertyChanged(nameof(Items));
		}
		/// <summary>
		/// Создаёт график. Список DataTime начла возможных записей.
		/// </summary>
		private List<DateTime> MakeSchedule(DateTime day) {
			List<DateTime> result = new List<DateTime>();
			List<DaySchedule> ScheduleList = (ExclusiveDays.Any(d => d.Date == day)
				? ExclusiveDays.Where(d => d.Date == day)
				: DaysSchedule.Where(d => d.DayOfWeak == (int)day.DayOfWeek % 7))
				.ToList();
			foreach(var schedule in ScheduleList) {
				DateTime start = day.Date + (schedule.Start); 
				DateTime end = day.Date + (schedule.End);
			for(DateTime time = start; time < end; time = time.AddMinutes(schedule.Interval)) 
				result.Add(time);
			}
			return result;
		}

		private bool IsWorkDay(DateTime day) {
			if(ExclusiveDays.Any(d => d.Date == day))
				return ExclusiveDays.Any(d => d.Date == day && d.IsWork);
			   
			if(DaysSchedule.Any(d => d.DayOfWeak == (int)day.DayOfWeek % 7))
				return DaysSchedule.Any(d => d.DayOfWeak == (int)day.DayOfWeek % 7 && d.IsWork);
			return false;
		}
		
		public void AddExpance(EmployeeCard employee, Visit visit) {
			navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder, EmployeeCard, Visit>(null, EntityUoWBuilder.ForCreate(), employee, visit);
		}
		
		public void OpenDocument(StockDocumentType type, int id) {
			IPage page;
			StockDocument doc;
			switch(type) {
				case StockDocumentType.ExpenseEmployeeDoc:
					navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(id));
					break;
				case StockDocumentType.WriteoffDoc:
					navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(id));
					break;
				case StockDocumentType.Return:
					navigation.OpenViewModel<ReturnViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(id));
					break;
				default: throw new NotSupportedException(type.ToString());
			}
		}

		public void NextDay() {
			Day = Day.AddDays(1);
			loadData();
			if(Items.Count == 0)
				NextDay();
			else 
				OnPropertyChanged(nameof(PeriodString));
		}
		public void PrevDay() {
			Day = Day.AddDays(-1);
			loadData();
			if(Items.Count == 0)
				PrevDay();
			else 
				OnPropertyChanged(nameof(PeriodString));
		}

		public void AddComment(VisitListWidgetItem item, string entryText) {
			if(item?.Visit != null && item.Visit.Comment != entryText) {
				item.Visit.Comment = entryText;
				UoW.Save(item.Visit);
				UoW.Commit();
			}
		}
	}
	
	public class VisitListWidgetItem : PropertyChangedBase {
	
		public VisitListWidgetItem(DateTime visitTime) {
			this.VisitTime = visitTime;
		}
		public VisitListWidgetItem(Visit visit) {
			if(visit != null) {
				this.Visit = visit;
				this.VisitTime = visit.VisitTime;
				this.Employee = visit.Employee;
			}
			else 
				throw new NullReferenceException(nameof(visit));
		}
		
		public DateTime VisitTime  { get; }
		
		public Visit Visit { get; }
		public DateTime? CreateTime => Visit?.CreateDate;
		public string Comment => Visit?.Comment;

		public string DocumentsString => string.Join("\n", Documents.Select(x => x.label));

		public List<(StockDocumentType doc, int id, string label)> Documents {
			get {
				var result = new List<(StockDocumentType, int, string)>();
				if(Visit?.ExpenseDocuments != null) 
					result.AddRange(Visit.ExpenseDocuments.Select(x => 
						(StockDocumentType.ExpenseEmployeeDoc, x.Id, $"{(x.IssueDate != null ? "Выдача" : "Черновик")} {x.DocNumberText}" )));
				if(Visit?.WriteoffDocuments != null)
					result.AddRange(Visit.WriteoffDocuments.Select(x => 
						(StockDocumentType.WriteoffDoc, x.Id,  $"Списание {x.DocNumberText}" )));
				if(Visit?.ReturnDocuments != null) 
					result.AddRange(Visit.ReturnDocuments.Select(x =>
						(StockDocumentType.Return, x.Id, $"Возврат {x.DocNumberText}" ))); 
				return result;
			}
		}
		
		public EmployeeCard Employee  { get; }
		public string FIO => Employee?.FullName ?? "";
		
	}
}
