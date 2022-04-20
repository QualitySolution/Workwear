using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using workwear.Domain.Stock;

namespace workwear.Domain.Regulations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры нормы",
		Nominative = "номенклатура нормы")]
	public class ProtectionTools : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название не должно быть пустым.")]
		[StringLength(240)]
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

		#region Аналоги

		private IList<ProtectionTools> analogs = new List<ProtectionTools>();

		[Display(Name = "Аналоги")]
		public virtual IList<ProtectionTools> Analogs {
			get { return analogs; }
			set { SetField(ref analogs, value, () => Analogs); }
		}

		GenericObservableList<ProtectionTools> observableAnalogs;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ProtectionTools> ObservableAnalog {
			get {
				if(observableAnalogs == null)
					observableAnalogs = new GenericObservableList<ProtectionTools>(Analogs);
				return observableAnalogs;
			}
		}

		public virtual void AddAnalog(ProtectionTools Analog)
		{
			if(Analogs.Any(p => DomainHelper.EqualDomainObjects(p, Analog))) {
				logger.Warn("Такой аналог уже добавлен. Пропускаем...");
				return;
			}
			if(DomainHelper.EqualDomainObjects(this, Analog)) {
				logger.Warn("Нельзя добавлять в качестве аналога себя. Пропускаем...");
				return;
			}
			ObservableAnalog.Add(Analog);
		}

		public virtual void RemoveAnalog(ProtectionTools Analog)
		{
			ObservableAnalog.Remove(Analog);
		}

		#region Расчетные

		public virtual IEnumerable<ProtectionTools> MatchedProtectionTools => (new[] { this }).Concat(Analogs);

		public virtual string GetAmountAndUnitsText(int amount)
		{
			return this?.Type.Units?.MakeAmountShortStr(amount) ?? amount.ToString();
		}

		#endregion

		#endregion

		#region Номенклатура

		private IList<Nomenclature> nomenclatures = new List<Nomenclature>();

		[Display(Name = "Номенклатура")]
		public virtual IList<Nomenclature> Nomenclatures {
			get { return nomenclatures; }
			set { SetField(ref nomenclatures, value, () => Nomenclatures); }
		}

		GenericObservableList<Nomenclature> observableNomenclatures;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Nomenclature> ObservableNomenclatures {
			get {
				if(observableNomenclatures == null)
					observableNomenclatures = new GenericObservableList<Nomenclature>(Nomenclatures);
				return observableNomenclatures;
			}
		}

		public virtual IEnumerable<Nomenclature> MatchedNomenclatures => Nomenclatures.Union(Analogs.SelectMany(x => x.Nomenclatures));

		public virtual void AddNomeclature(Nomenclature nomenclature)
		{
			if(Nomenclatures.Any(p => DomainHelper.EqualDomainObjects(p, nomenclature))) {
				logger.Warn("Номеклатура уже добавлена. Пропускаем...");
				return;
			}
			ObservableNomenclatures.Add(nomenclature);
		}

		public virtual void RemoveNomeclature(Nomenclature nomenclature)
		{
			ObservableNomenclatures.Remove(nomenclature);
		}

		#endregion
	}
}
