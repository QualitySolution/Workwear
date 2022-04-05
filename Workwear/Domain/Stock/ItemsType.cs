using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using workwear.Domain.Sizes;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы номенклатуры",
		Nominative = "тип номенклатуры",
		Genitive = "типа номенклатуры"
		)]
	[HistoryTrace]
	public class ItemsType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		private string name;
		[Display (Name = "Название")]
		[Required(ErrorMessage = "Имя типа номенклатуры не должно быть пустым.")]
		[StringLength(240)]
		public virtual string Name {
			get => name;
			set => SetField (ref name, value?.Trim());
		}

		private MeasurementUnits units;
		[Display (Name = "Единица измерения")]
		[Required(ErrorMessage = "Единица измерения должна быть указана.")]
		public virtual MeasurementUnits Units {
			get => units;
			set => SetField (ref units, value);
		}

		private ItemTypeCategory category;
		[Display (Name = "Категория")]
		public virtual ItemTypeCategory Category {
			get => category;
			set {
				if (!SetField(ref category, value, () => Category)) return;
				if (Category != ItemTypeCategory.wear) {
					SizeType = null;
					HeightType = null;
				}
				if (Category != ItemTypeCategory.property)
					LifeMonths = null;
			}
		}
		
		private IssueType issueType;
		[Display(Name = "Тип выдачи")]
		public virtual IssueType IssueType {
			get => issueType;
			set => SetField(ref issueType, value);
		}

		private int? lifeMonths;
		[Display (Name = "Срок службы")]
		public virtual int? LifeMonths {
			get => lifeMonths;
			set => SetField (ref lifeMonths, value);
		}
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => comment;
			set => SetField(ref comment, value);
		}
		private IList<Nomenclature> nomenclatures = new List<Nomenclature>();
		[Display(Name = "Номенклатура")]
		public virtual IList<Nomenclature> Nomenclatures {
			get => nomenclatures;
			set => SetField(ref nomenclatures, value);
		}
		GenericObservableList<Nomenclature> observableNomenclatures;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Nomenclature> ObservableNomenclatures =>
			observableNomenclatures ??
			(observableNomenclatures = new GenericObservableList<Nomenclature>(Nomenclatures));

		private SizeType sizeType;
		[Display(Name = "Тип размера")]
		public virtual SizeType SizeType {
			get => sizeType;
			set => SetField(ref sizeType, value);
		}
		private SizeType heightType;
		[Display(Name = "Тип роста")]
		public virtual SizeType HeightType {
			get => heightType;
			set => SetField(ref heightType, value);
		}
		#endregion
		public ItemsType () { }
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			if (Category == ItemTypeCategory.wear && SizeType is null)
				yield return new ValidationResult ("Вид размера одежды должен быть указан.", 
					new[] { this.GetPropertyName (o => o.SizeType)});
		}
		#endregion
	}
	public enum ItemTypeCategory{
		[Display(Name = "Спецодежда")]
		wear,
		[Display(Name = "Имущество")]
		property
	}

	public enum IssueType {
		[Display(Name = "Персональная")]
		Personal,
		[Display(Name = "Коллективная")]
		Collective
	}
}



