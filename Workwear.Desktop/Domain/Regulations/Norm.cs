﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using NLog;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Company;

namespace Workwear.Domain.Regulations
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "нормы выдачи",
		Nominative = "норма выдачи",
		PrepositionalPlural = "нормах выдачи",
		Genitive = "нормы выдачи"
	)]
	[HistoryTrace]
	public class Norm : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
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

		string tonParagraph;
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
		
		private string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		DateTime? dateFrom;
		[Display(Name = "Начало действия")]
		public virtual DateTime? DateFrom {
			get => dateFrom;
			set => SetField(ref dateFrom, value);
		}

		DateTime? dateTo;
		[Display(Name = "Окончание действия")]
		public virtual DateTime? DateTo {
			get => dateTo;
			set => SetField(ref dateTo, value);
		}
		#endregion
		#region Коллеции
		private IObservableList<Post> posts = new ObservableList<Post>();
		[Display (Name = "Должности")]
		public virtual IObservableList<Post> Posts {
			get => posts;
			set => SetField (ref posts, value);
		}

		private IObservableList<NormItem> items = new ObservableList<NormItem>();
		[Display (Name = "Строки норм")]
		public virtual IObservableList<NormItem> Items {
			get => items;
			set => SetField (ref items, value);
		}
		#endregion
		#region Генерируемые
		public virtual string ProfessionsText {
			get{ return String.Join ("; ", Posts.Select (p => p.Name));
			}
		}

		public virtual string DocumentNumberText => document?.Number;
		public virtual string AnnexNumberText => Annex?.Number.ToString();
		public virtual bool IsActive => (DateFrom == null || DateFrom.Value <= DateTime.Today)
		                                && (DateTo == null || DateTo >= DateTime.Today);

		public virtual string Title => Name ?? $"{DocumentNumberText} {AnnexNumberText} {TONParagraph}";
		#endregion

		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			if (Items.Count == 0)
				yield return new ValidationResult ("Норма должна содержать хотя бы одну номенклатуру.", 
					new[] { this.GetPropertyName (o => o.Items) });
			foreach(var item in items) {
				if(item.PeriodCount <= 0 && item.NormPeriod != NormPeriodType.Wearout && item.NormPeriod != NormPeriodType.Duty){
					yield return new ValidationResult(
						$"Период эксплуатации номенклатуры {item.ProtectionTools.Name} должен быть больше нуля.",
					new[] { nameof(item.PeriodCount) });
				}
				if(item.PeriodCount > 255){
					yield return new ValidationResult("Указан слишком большой период эксплуатации номенклатуры " +
					                                  $"{item.ProtectionTools.Name}, перепроверьте введеные данные",
						new[] { nameof(item.PeriodCount) });
				}
				if(item.Amount <= 0) {
					yield return new ValidationResult(
						$"Количество у номенклатуры {item.ProtectionTools.Name} должно быть больше нуля.",
					new[] { nameof(item.PeriodCount) });
				}
			}
		}

		#endregion
		public virtual void AddPost(Post prof) {
			if(Posts.Any (p => DomainHelper.EqualDomainObjects (p, prof)))
			{
				logger.Warn ("Такая профессия уже добавлена. Пропускаем...");
				return;
			}
			Posts.Add (prof);
		}

		public virtual void RemovePost(Post prof) {
			Posts.Remove (prof);
		}

		public virtual NormItem AddItem(ProtectionTools tools) {
			if(Items.Any (i => DomainHelper.EqualDomainObjects (i.ProtectionTools, tools))) {
				logger.Warn ("Такое наименование уже добавлено. Пропускаем...");
				return null;
			}

			var item = new NormItem {
				Norm = this,
				ProtectionTools = tools,
				Amount = 1,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 1
			};

			Items.Add (item);
			return item;
		}

		public virtual void RemoveItem(NormItem item) {
			Items.Remove (item);
		}

		/// <summary>
		/// Присваивает newNorm копию текущего объекта Norm 
		/// </summary>
		/// <param name="newNorm">Ссылка на норму, в которую необходимо скопировать текущую норму</param>
		public virtual void CopyNorm(Norm newNorm) {
			newNorm.Document = document;
			newNorm.Annex = annex;
			newNorm.TONParagraph= tonParagraph;
			newNorm.Name = name;
			//тут передобавлять
			foreach(var item in posts) {
				newNorm.Posts.Add(item);
			}
			//тут передобавлять
			foreach(var item in items.Select(i => i.CopyNormItem(newNorm))) {
				newNorm.Items.Add(item);
			}
			newNorm.Comment = comment;
			newNorm.DateFrom = dateFrom;
			newNorm.DateTo = dateTo;
		}
	}
}

