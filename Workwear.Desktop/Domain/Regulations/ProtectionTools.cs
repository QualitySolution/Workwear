using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Analytics;
using Workwear.Domain.Company;
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

		private ProtectionToolsCategory categoryForAnalytic;
		[Display(Name = "Категория номенклатуры нормы для аналитики")]
		public virtual ProtectionToolsCategory CategoryForAnalytic 
		{
			get => categoryForAnalytic;
			set => SetField(ref categoryForAnalytic, value);
		}
		
		private SupplyType supplyType;
		[Display(Name = "Тип закупки")]
		public virtual SupplyType SupplyType 
		{
			get => supplyType;
			set => SetField(ref supplyType, value);
		}
		private Nomenclature supplyNomenclatureUnisex;
		[Display(Name = "Закупаемая номенклатура, унисекс")]
		public virtual Nomenclature SupplyNomenclatureUnisex 
		{
			get => supplyNomenclatureUnisex;
			set => SetField(ref supplyNomenclatureUnisex, value);
		}
		private Nomenclature supplyNomenclatureMale;
		[Display(Name = "Закупаемая номенклатура, мужская")]
		public virtual Nomenclature SupplyNomenclatureMale
		{
			get => supplyNomenclatureMale;
			set => SetField(ref supplyNomenclatureMale, value);
		}
		private Nomenclature supplyNomenclatureFemale;
		[Display(Name = "Закупаемая номенклатура, женская")]
		public virtual Nomenclature SupplyNomenclatureFemale
		{
			get => supplyNomenclatureFemale;
			set => SetField(ref supplyNomenclatureFemale, value);
		}
		#endregion
		#region Расчетные
		public virtual string GetAmountAndUnitsText(int amount) => this?.Type.Units?.MakeAmountShortStr(amount) ?? amount.ToString();

		public virtual Nomenclature GetSupplyNomenclature(Sex? sex) {
			if(SupplyType == SupplyType.Unisex)
				return SupplyNomenclatureUnisex;
			if(sex == Sex.F)
				return SupplyNomenclatureFemale;
			if(sex == Sex.M)
				return SupplyNomenclatureMale;
			return null;
		}
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

	public enum SupplyType 	{		
		[Display(Name = "Универсально")]
		Unisex,
		[Display(Name = "Муж./Жен.")]
		TwoSex,
	}
}
