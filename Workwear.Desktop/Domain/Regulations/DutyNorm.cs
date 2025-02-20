using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Regulations {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "дежурные нормы",
		Nominative = "дежурная норма",
		PrepositionalPlural = "дежурных нормах",
		Genitive = "дежурной нормы"
	)]
	[HistoryTrace]
	public class DutyNorm : PropertyChangedBase, IDomainObject {
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public virtual IEnumerable<ProtectionTools> ProtectionToolsList => Items.Select(x => x.ProtectionTools);
			
		#region Хранимые Свойства

		public virtual int Id { get; set; }

		private Leader responsibleLeader;
		[Display(Name = "Ответственное должностное лицо")]
		public virtual Leader ResponsibleLeader {
			get => responsibleLeader;
			set => SetField(ref responsibleLeader, value);
		}

		private EmployeeCard responsibleEmployee;
		[Display(Name = "Ответственный сотрудник")]
		public virtual EmployeeCard ResponsibleEmployee {
			get => responsibleEmployee;
			set => SetField(ref responsibleEmployee, value);
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private string name;
		[Display(Name = "Название")]
		[StringLength(200)]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}
		
		private DateTime? dateFrom;
		[Display(Name = "Начало действия")]
		public virtual DateTime? DateFrom {
			get => dateFrom;
			set => SetField(ref dateFrom, value);
		}

		private DateTime? dateTo;
		[Display(Name = "Окончание действия")]
		public virtual DateTime? DateTo {
			get => dateTo;
			set => SetField(ref dateTo, value);
		}
		
//TODO Не реализовано				
		private bool archive;
		[Display(Name = "Архивная(отключена)")]
		public virtual bool Archive {
			get => archive;
			set => SetField(ref archive, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private IObservableList<DutyNormItem> items = new ObservableList<DutyNormItem>();
		[Display(Name = "Строки дежурных норм")]
		public virtual IObservableList<DutyNormItem> Items {
			get => items;
			set => SetField(ref items, value);
		}
		#endregion
		
		#region Методы
		
		public virtual DutyNormItem AddItem(ProtectionTools tools) {
			if(Items.Any (i => DomainHelper.EqualDomainObjects (i.ProtectionTools, tools))) {
				logger.Warn ("Такое наименование уже добавлено. Пропускаем...");
				return null;
			}

			var item = new DutyNormItem {
				DutyNorm = this,
				ProtectionTools = tools,
				Amount = 1,
				NormPeriod = DutyNormPeriodType.Year,
				PeriodCount = 1,
				Graph = new IssueGraph(),
			};

			Items.Add (item);
			return item;
		}

		public virtual DutyNormItem GetItem(ProtectionTools protectionTools) {
			if(protectionTools != null) {
				return Items.FirstOrDefault(i => i.ProtectionTools == protectionTools);
			}
			return null;
		}
		public virtual DutyNormItem GetItem(Nomenclature nomenclature) {
			if(nomenclature != null) {
				return Items.FirstOrDefault(i => i.ProtectionTools.Nomenclatures
					.Any(n => DomainHelper.EqualDomainObjects(n, nomenclature)));
			}
			return null;
		}

		public virtual void UpdateItems(IUnitOfWork uow) {
			foreach(var item in items)
				item.Update(uow);
			OnPropertyChanged(nameof(Items));
		}
		#endregion

	}
}
