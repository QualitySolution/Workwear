using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock {
	public class NomenclatureSizes : PropertyChangedBase, IDomainObject {
		
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
		public virtual Size Size {
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
		#endregion
	}
}
