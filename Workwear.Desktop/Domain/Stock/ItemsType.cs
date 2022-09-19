using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы номенклатуры",
		Nominative = "тип номенклатуры",
		Genitive = "типа номенклатуры"
		)]
	[HistoryTrace]
	public class ItemsType : PropertyChangedBase, IDomainObject
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
		
		СlothesType? wearCategory;
		[Display (Name = "Вид одежды")]
		public virtual СlothesType? WearCategory {
			get { return wearCategory; }
			set { SetField (ref wearCategory, value, () => WearCategory); }
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
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
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
	public enum СlothesType
	{
		[Display(Name = "Одежда")]
		Wear,
		[Display(Name = "Обувь")]
		Shoes,
		[Display(Name = "Зимняя обувь")]
		WinterShoes,
		[Display(Name = "Головные уборы")]
		Headgear,
		[Display(Name = "Перчатки")]
		Gloves,
		[Display(Name = "Рукавицы")]
		Mittens,
		[Display(Name = "СИЗ")]
		PPE
	}
}



