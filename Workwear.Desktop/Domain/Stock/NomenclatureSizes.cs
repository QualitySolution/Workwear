using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Stock {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "размеры номенклатуры",
		Nominative = "размер номенклатуры"
	)]
	public class NomenclatureSizes : PropertyChangedBase, IDomainObject, IDocItemSizeInfo {
		
		#region Cвойства
		public virtual int Id { get; set; }

		private Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}
		
		private Size wearSize;
		[Display(Name = "Размер")]
		public virtual Size WearSize {
			get => wearSize;
			set => SetField(ref wearSize, value);
		}
		
		private Size height;

		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => SetField(ref height, value);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		
		public virtual SizeType WearSizeType => Nomenclature?.Type?.SizeType;
		public virtual SizeType HeightType => Nomenclature?.Type?.HeightType;
		public virtual int Amount { get; set; } = 0;
		public virtual string Title => WearSize?.Name + (WearSize != null && Height != null ? "/" : "") + Height?.Name;

		#endregion
	}
}
