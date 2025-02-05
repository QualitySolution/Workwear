using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Report;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Regulations {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "дежурные нормы",
		Nominative = "дежурная норма",
		PrepositionalPlural = "дежурных нормах",
		Genitive = "дежурые нормы"
	)]
	[HistoryTrace]
	public class DutyNorm : PropertyChangedBase, IDomainObject {
		private static Logger logger = LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		private RegulationDoc document;
		[Display(Name = "Нормативный документ")]
		public virtual RegulationDoc Document {
			get => document;
			set => SetField(ref document, value);
		}

		private RegulationDocAnnex annex;
		[Display(Name = "Приложение нормативного документа")]
		public virtual RegulationDocAnnex Annex {
			get => annex;
			set => SetField(ref annex, value);
		}

		private string tonParagraph;
		[Display(Name = "№ пункта приложения ТОН")]
		[StringLength(15)]
		public virtual string TONParagraph {
			get => tonParagraph;
			set => SetField(ref tonParagraph, value);
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

		#endregion
		
		#region Строки нормы
		private IObservableList<DutyNormItem> items = new ObservableList<DutyNormItem>();

		[Display(Name = "Строки дежурных норм")]
		public virtual IObservableList<DutyNormItem> Items {
			get => items;
			set => SetField(ref items, value);
		}
		
		public virtual DutyNormItem AddItem(ProtectionTools tools) {
			if(Items.Any (i => DomainHelper.EqualDomainObjects (i.ProtectionTools, tools))) {
				logger.Warn ("Такое наименование уже добавлено. Пропускаем...");
				return null;
			}

			var item = new DutyNormItem {
				DutyNorm = this,
				ProtectionTools = tools,
				Amount = 1,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 1,
				Graph = new IssueGraph<DutyNormIssueOperation>(),
			};

			Items.Add (item);
			return item;
		}
		#endregion

		#region Методы

		public virtual DutyNormItem GetItem(ProtectionTools protectionTools) {
			if(protectionTools != null) {
				return Items.FirstOrDefault(i => i.ProtectionTools == protectionTools);
			}
			return null;
		}
		public virtual DutyNormItem GetItem(Nomenclature nomenclature) {
			if(nomenclature != null) {
				var pto = items.Select(i => i.ProtectionTools)
					.FirstOrDefault(pt => pt.Nomenclatures.Select(n => n.Id).Contains(nomenclature.Id));
				return GetItem(pto);
			}
			return null;
		}
		
		public virtual void UpdateItems(IUnitOfWork uow) {
			bool change = false;
			foreach (var item in items)
				change = item.Update(uow) || change;
			if(change)// Чтобы не тригерить Хибернейт
				OnPropertyChanged(nameof(Items));
		}
		#endregion
		public virtual IEnumerable<ProtectionTools> ProtectionToolsList => Items.Select(x => x.ProtectionTools);
	}
	
	public enum DutyNormSheetPrint
	{
		[Display(Name = "Лицевая сторона")]
		[ReportIdentifier("DutyNorms.DutyNormPage1")]
		DutyNormPage1,
		[Display(Name = "Обратная сторона")]
		[ReportIdentifier("DutyNorms.DutyNormPage2")]
		DutyNormPage2,
	}
}
