using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "штрихкоды",
		Nominative = "штрихкод",
		Genitive = "штрихкода"
		)]
	[HistoryTrace]
	public class Barcode : PropertyChangedBase, IDomainObject
	{
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
	}
}
