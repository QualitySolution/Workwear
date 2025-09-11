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
using Workwear.Models.Visits;
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
			
			listModel = new VisitListModel(UoW.GetAll<DaySchedule>().ToList());

			//Для обновления вюшки при действиях с документами
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Expense>();
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Return>();
			changeWatcher.BatchSubscribe(VisitChangeEvent).IfEntity<Writeoff>();

			loadData();
			if(listModel.Items.Count == 0)
				NextDay();
		}
		//TODO докумментация IDialogDocumentation
		//public string DocumentationUrl { get; }
		//public string ButtonTooltip { get; }
		
		private readonly INavigationManager navigation;
		private readonly IEntityChangeWatcher changeWatcher;
		private VisitListModel listModel;
		public SortedDictionary<DateTime, VisitListItem> Items => listModel.Items; 
		public DateTime Day { get; set; } = DateTime.Now.Date;
		public string PeriodString => Day.Date.ToShortDateString();

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

			listModel.PutVisits(UoW.GetAll<Visit>()
				.Where(x => x.VisitTime >= Day)
				.Where(x => x.VisitTime < Day.AddDays(1))
				.Fetch(x => x.Employee)
				.ToList()
			);

			if(listModel.IsWorkDay(Day))
				listModel.FillScheduleOfDay(Day);
			
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

		public void AddComment(VisitListItem item, string entryText) {
			if(item?.Visit != null && item.Visit.Comment != entryText) {
				item.Visit.Comment = entryText;
				UoW.Save(item.Visit);
				UoW.Commit();
			}
		}
	}
}
