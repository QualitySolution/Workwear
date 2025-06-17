using System;
using System.Collections.Generic;
using System.Linq;
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
using Workwear.Tools;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Visits {
	public class VisitListViewModel : UowDialogViewModelBase 
	{
		public VisitListViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEntityChangeWatcher changeWatcher,
			INavigationManager navigation,
			BaseParameters baseParameters,
			IValidator validator = null,
            UnitOfWorkProvider unitOfWorkProvider = null,
			string UoWTitle = null
			) : base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.changeWatcher = changeWatcher ?? throw new ArgumentNullException(nameof(changeWatcher));
			
			Title = "Посещения склада";
			IntervalMinutes = baseParameters.VisitInterval;
			StartWorkDay = baseParameters.VisitIntervalHourStart;
			FinishWorkDay = baseParameters.VisitIntervalHourEnd;
			WorkDaysOfWeak = baseParameters.VisitIntervalWorkDays;
			NotWorkDays = UoW.GetAll<WorkDay>()
				.Where(d => !d.IsWorkday)
				.Select(d => d.Date).ToArray();

			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Expense>();
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Return>();
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Writeoff>();
			
			loadData();
		}
		//TODO докумментация IDialogDocumentation
		//public string DocumentationUrl { get; }
		//public string ButtonTooltip { get; }
		
		private readonly INavigationManager navigation;
		private readonly IEntityChangeWatcher changeWatcher;
		public DateTime StartPeriod { get; set; } = DateTime.Now.Date;
		public string PeriodString => StartPeriod.Date.ToShortDateString();
		public DateTime FinishPeriod => StartPeriod.AddDays(1);
		public int IntervalMinutes { get; set; } = 15;
		public int StartWorkDay { get; set; } = 8;
		public int FinishWorkDay { get; set; } = 17;
		public int[] WorkDaysOfWeak { get; set; } = {1, 2, 3, 4, 5};
		public DateTime[] NotWorkDays { get; }
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

			foreach(var v in Items.Where(x => x.Value.Visit != null)) 
				UoW.Session.Evict(v.Value.Visit);
			Items.Clear();
	
			Visit visitAlias = null;
			EmployeeCard employeeAlias = null;
			var visits = UoW.GetAll<Visit>() 
				.Where(x => x.VisitTime >= StartPeriod)
				.Where(x => x.VisitTime < FinishPeriod)
				.ToList<Visit>();
			
			foreach(var visit in visits) {
				while(Items.ContainsKey(visit.VisitTime))
					visit.VisitTime.AddSeconds(1);
				Items.Add(visit.VisitTime, new VisitListWidgetItem(visit));
			}

			for(DateTime d = StartPeriod; d.Date < FinishPeriod; ) {
				if(d.Hour < StartWorkDay) {
					d = d.Date.AddHours(StartWorkDay); 
					continue; 
				}
				if(!WorkDaysOfWeak.Contains((int)d.DayOfWeek) || d.Hour >= FinishWorkDay || NotWorkDays.Contains(d.Date)) {
					d = d.AddDays(1).Date.AddHours(StartWorkDay); 
					continue; 
				}
				if (!Items.ContainsKey(d))
					Items.Add(d, new VisitListWidgetItem(d));
				
				d = d.AddMinutes(IntervalMinutes); // Счётчик цикла!
			}

			//Помечаем начало дня
			Items.GroupBy(x => x.Key.Date)
				.Select(g => g.OrderBy(y => y.Key).First()).ToList()
				.ForEach(i => i.Value.FirstOfDay = true);
			
			OnPropertyChanged(nameof(Items));
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
			StartPeriod = StartPeriod.AddDays(1);
			loadData();
			if(Items.Count == 0)
				NextDay();
			else 
				OnPropertyChanged(nameof(PeriodString));
		}
		public void PrevDay() {
			StartPeriod = StartPeriod.AddDays(-1);
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
						(StockDocumentType.ExpenseEmployeeDoc, x.Id, $"{(x.IssueDate != null ? "Выдача" : "Черовик")} {x.DocNumberText}" )));
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

		public bool FirstOfDay { get; set; }
	}
}
