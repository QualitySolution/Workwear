using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Postomats {
	public class PostomatDocumentItem : PropertyChangedBase{
		#region Cвойства
		public virtual int Id { get; set; }
		
		public virtual PostomatDocument Document { get; set; }
		
		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		private Barcode barcode;
		[Display(Name = "Штрихкод")]
		public virtual Barcode Barcode {
			get => barcode;
			set => SetField(ref barcode, value);
		}
		
		private int delta;
		[Display(Name = "Изменение количества")]
		public virtual int Delta {
			get => delta;
			set => SetField(ref delta, value);
		}

		private uint locationStorage;
		[Display(Name = "Устройство")]
		public virtual uint LocationStorage {
			get => locationStorage;
			set => SetField(ref locationStorage, value);
		}
		
		private uint locationShelf;
		[Display(Name = "Полка")]
		public virtual uint LocationShelf {
			get => locationShelf;
			set => SetField(ref locationShelf, value);
		}
		
		private uint locationCell;
		[Display(Name = "Ячейка")]
		public virtual uint LocationCell {
			get => locationCell;
			set => SetField(ref locationCell, value);
		}
		#endregion
	}
}
