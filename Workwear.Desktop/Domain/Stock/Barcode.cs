using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "штрихкоды",
		Nominative = "штрихкод",
		Genitive = "штрихкода",
		GenitivePlural = "штрихкодов"
	)]
	[HistoryTrace]
	public class Barcode : PropertyChangedBase, IDomainObject {
		public virtual int Id { get; }

		private DateTime createDate = DateTime.Today;

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate {
			get => createDate;
			set => SetField(ref createDate, value);
		}
		
		private string title;
		[Display (Name = "Значение")]
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private BarcodeTypes type;
		[Display(Name = "Тип метки")]
		public virtual BarcodeTypes Type {
			get => type;
			set => SetField(ref type, value);
		}


		private Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}
		
		private Size size;
		[Display(Name = "Размер")]
		public virtual Size Size {
			get => size;
			set => SetField(ref size, value);
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
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}
		
		#region Списки

		private IList<BarcodeOperation> barcodeOperations = new List<BarcodeOperation>();
		[Display(Name = "Операции")]
		public virtual IList<BarcodeOperation> BarcodeOperations {
			get => barcodeOperations;
			set => SetField(ref barcodeOperations, value);
		}

		#endregion
	}
	
	public enum BarcodeTypes {
		[Display(Name = "Линейный EAN-13")]
		EAN13,
		[Display(Name ="RFID EPC 96bit ")]
		EPC96 
	}
}
