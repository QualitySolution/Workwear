using System.ComponentModel.DataAnnotations;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Supply{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки предполагаемой поставки",
		Nominative = "строка предполагаемой поставки",
		Genitive = "строки предполагаемой поставки"
	)]
	[HistoryTrace]
	public class ShipmentItem: IDomainObject 
	{
		#region Свойства
		public virtual int Id { get; set; }
		
		private Shipment shipment;
		
		[Display(Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Shipment Shipment {
			get => shipment;
			set {shipment = value;}
		}
		
		Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { nomenclature = value; }
		}
		
		private int requested;
		[Display (Name = "Количество запрошено")]
		[PropertyChangedAlso(nameof(TotalRequested))]
		public virtual int Requested {
			get => requested;
			set {requested = value; }
		}	
		
		private int ordered;
		[Display (Name = "Количество заказано")]
		[PropertyChangedAlso(nameof(TotalOrdered))]
		public virtual int Ordered {
			get => ordered;
			set {ordered = value; }
		}
		
		private int received;
		[Display (Name = "Количество поставлено")]
		[PropertyChangedAlso(nameof(TotalRequested))]
		public virtual int Received {
			get => received;
            set {received = value; }
		}

		private decimal cost;
		[Display (Name = "Цена")]
		[PropertyChangedAlso(nameof(TotalOrdered))]
		[PropertyChangedAlso(nameof(TotalRequested))]
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
			get => comment;
			set { comment = value; }
		}
		
		private string diffСause;
		[Display(Name = "Причина расхождений")]
		public virtual string DiffСause {
			get => diffСause;
			set { diffСause = value; }
		}
		#endregion
		
		#region Расчетные свойства

		public virtual string Title => $"Закупка {Nomenclature?.Name} в количестве {Requested} {Nomenclature?.Type?.Units?.Name}";
		public virtual decimal TotalRequested => Cost * Requested;
		public virtual decimal TotalOrdered => Cost * Ordered;
		public virtual StockPosition StockPosition => 
			new StockPosition(Nomenclature, 0m,WearSize, Height, null);
		
		/// <summary>
		/// Заказано, но еще не получено.
		/// </summary>
		public virtual int OrderedNotReceived => Ordered - Received;
		
		[Display (Name = "Наименование")]
		public virtual string ItemName => nomenclature?.Name;

		[Display(Name = "Тип Роста")]
		public virtual SizeType HeightType => nomenclature?.Type.HeightType;

		[Display(Name = "Тип размера одежды")]
		public virtual SizeType WearSizeType => nomenclature?.Type.SizeType;

		[Display(Name = "Единица измерения")]
		public virtual MeasurementUnits Units => nomenclature?.Type.Units;

		#endregion
		public ShipmentItem(){ }

		public ShipmentItem(Shipment shipment) {
			this.shipment = shipment;
		}
	}
}
