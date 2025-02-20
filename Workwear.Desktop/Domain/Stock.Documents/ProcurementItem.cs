using System.ComponentModel.DataAnnotations;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки предполагаемой поставки",
		Nominative = "строка предполагаемой поставки",
		Genitive = "строки предполагаемой поставки"
	)]
	[HistoryTrace]
	public class ProcurementItem: IDomainObject 
	{
		#region Свойства
		public virtual int Id { get; set; }
		
		private Procurement procurement;
		
		[Display(Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Procurement Procurement {
			get => procurement;
			set {procurement = value;}
		}
		
		Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { nomenclature = value; }
		}
		
		[Display (Name = "Наименование")]
		public virtual string ItemName {
			get => nomenclature?.Name; 
		}
		
		[Display(Name = "Тип Роста")]
		public virtual SizeType HeightType {
			get => nomenclature?.Type.HeightType;
		}
		
		[Display(Name = "Тип размера одежды")]
		public virtual SizeType WearSizeType {
			get => nomenclature?.Type.SizeType;
		}
		
		[Display(Name = "Единица измерения")]
		public virtual MeasurementUnits Units {
			get => nomenclature?.Type.Units; 
		}
		
		private int amount;
		[Display (Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get => amount;
			set {amount = value; }
		}

		private decimal cost;
		[Display (Name = "Цена")]
		[PropertyChangedAlso("Total")]
		public virtual decimal Cost {
			get => cost;
			set { cost = value; }
		}
		
		private Size wearSize;
		[Display(Name = "Размер")]
		public virtual Size WearSize {
			get => wearSize;
			set => wearSize = value;
		}
		
		private Size height;
		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => height = value;
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get =>comment;
			set { comment = value; }
		}
		#endregion
		
		#region Расчетные

		public virtual string Title => $"Закупка {Nomenclature?.Name} в количестве {Amount} {Nomenclature?.Type?.Units?.Name}";
		public virtual decimal Total => Cost * Amount;
		public virtual StockPosition StockPosition => 
			new StockPosition(Nomenclature, 0m,WearSize, Height, Owner);

		public Owner Owner { get; set; }

		#endregion

		public ProcurementItem(){ }

		public ProcurementItem(Procurement procurement) {
			this.procurement = procurement;
		}
	}
}
