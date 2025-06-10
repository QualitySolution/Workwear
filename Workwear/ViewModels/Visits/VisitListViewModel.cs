using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Visits {
	public class VisitListViewModel : UowDialogViewModelBase, IDialogDocumentation
	{
		public VisitListViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
            UnitOfWorkProvider unitOfWorkProvider = null,
			string UoWTitle = null
			) : base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

			NotWorkDays = UoW.GetAll<WorkDay>()
				.Where(d => !d.IsWorkday)
				.Select(d => d.Date).ToArray();
			
			loadData();
		}
		//TODO докумментация
		public string DocumentationUrl { get; }
		public string ButtonTooltip { get; }
		
		private readonly INavigationManager navigation;
		public DateTime StartPeriod { get; set; } = DateTime.Now.Date;
		public string PeriodString => StartPeriod.Date.ToShortDateString();
		public DateTime FinishPeriod => StartPeriod.AddDays(1);
		public int IntervalMinutes { get; set; } = 15;
		public int StartWorkDay { get; set; } = 8;
		public int FinishWorkDay { get; set; } = 17;
		public int[] WorkDaysOfWeak { get; set; } = {1, 2, 3, 4, 5};
		
		public DateTime[] NotWorkDays { get; }

		private void loadData() {
			Items.Clear();
	
			Visit visitAlias = null;
			EmployeeCard employeeAlias = null;
			var visits = UoW.Session.QueryOver<Visit>()
				.Fetch(SelectMode.ChildFetch, x => x)
				.Where(x => x.VisitTime >= StartPeriod)
				.Where(x => x.VisitTime < FinishPeriod)
				.JoinAlias(x => x.Employee, () => employeeAlias)
				.List<Visit>();
			
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

		public SortedDictionary<DateTime, VisitListWidgetItem> Items { get; set; } = new SortedDictionary<DateTime, VisitListWidgetItem>();
		
		public void AddExpance(EmployeeCard employee, Visit visit) {
			var page = navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder, EmployeeCard, Visit>(null, EntityUoWBuilder.ForCreate(), employee, visit);
			page.PageClosed += (sender, args) => loadData();
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
		public string Documents => string.Join("\n",(Visit?.ExpenseDocuments?.Select(x => x.Title) ?? Array.Empty<string>())
			.Union(Visit?.WriteoffDocuments?.Select(x => x.Title) ?? Array.Empty<string>())
			.Union(Visit?.ReturnDocuments?.Select(x => x.Title) ?? Array.Empty<string>()));
		
		public EmployeeCard Employee  { get; }
		public string FIO => Employee?.FullName ?? "";

		public bool FirstOfDay { get; set; }
	}
}
