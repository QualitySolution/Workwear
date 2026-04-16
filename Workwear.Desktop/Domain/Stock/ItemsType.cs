using System.ComponentModel.DataAnnotations;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы номенклатуры",
		Nominative = "тип номенклатуры",
		Genitive = "типа номенклатуры",
		GenitivePlural = "типов номенклатуры"
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
		
		ClothesType? wearCategory;
		[Display (Name = "Вид одежды")]
		public virtual ClothesType? WearCategory {
			get { return wearCategory; }
			set { SetField (ref wearCategory, value, () => WearCategory); }
		}
		
		private IssueType issueType;
		[Display(Name = "Тип выдачи")]
		public virtual IssueType IssueType {
			get => issueType;
			set => SetField(ref issueType, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => comment;
			set => SetField(ref comment, value);
		}
		private IObservableList<Nomenclature> nomenclatures = new ObservableList<Nomenclature>();
		[Display(Name = "Номенклатура")]
		public virtual IObservableList<Nomenclature> Nomenclatures {
			get => nomenclatures;
			set => SetField(ref nomenclatures, value);
		}

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

	public enum IssueType {
		[Display(Name = "Персональная")]
		Personal,
		[Display(Name = "Коллективная")]
		Collective
	}
	public enum ClothesType
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



