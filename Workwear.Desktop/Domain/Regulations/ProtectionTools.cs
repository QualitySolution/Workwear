﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Regulations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры нормы",
		Nominative = "номенклатура нормы",
		Genitive = "номенклатуры нормы"
		)]
	[HistoryTrace]
	public class ProtectionTools : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название не должно быть пустым.")]
		[StringLength(800)]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value?.Trim()); }
		}

		ItemsType type;

		[Display(Name = "Группа номенклатуры")]
		[Required(ErrorMessage = "Группа номенклатуры должна быть указана.")]
		public virtual ItemsType Type {
			get { return type; }
			set { SetField(ref type, value, () => Type); }
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value); }
		}
		private decimal? assessedCost;
		[Display(Name ="Оценочная стоимость")]
		public virtual decimal? AssessedCost {
			get { return assessedCost; }
			set { SetField(ref assessedCost, value); }
		}

		#endregion
		#region Расчетные
		public virtual string GetAmountAndUnitsText(int amount) => this?.Type.Units?.MakeAmountShortStr(amount) ?? amount.ToString();
		#endregion

		#region Номенклатура

		private IObservableList<Nomenclature> nomenclatures = new ObservableList<Nomenclature>();

		[Display(Name = "Номенклатура")]
		public virtual IObservableList<Nomenclature> Nomenclatures {
			get { return nomenclatures; }
			set { SetField(ref nomenclatures, value, () => Nomenclatures); }
		}

		public virtual void AddNomenclature(Nomenclature nomenclature)
		{
			if(Nomenclatures.Any(p => DomainHelper.EqualDomainObjects(p, nomenclature))) {
				logger.Warn("Номеклатура уже добавлена. Пропускаем...");
				return;
			}
			Nomenclatures.Add(nomenclature);
		}

		public virtual void RemoveNomenclature(Nomenclature nomenclature)
		{
			Nomenclatures.Remove(nomenclature);
		}
		#endregion
	}
}
