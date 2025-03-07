﻿using System.ComponentModel.DataAnnotations;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки прихода",
		Nominative = "строка прихода",
		Genitive = "строки прихода"
		)]
	[HistoryTrace]
	public class IncomeItem : PropertyChangedBase, IDomainObject , IDocItemSizeInfo
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Income document;

		[Display(Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Income Document {
			get => document;
			set => SetField(ref document, value);
		}

		Nomenclature nomenclature;
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
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
			set { SetField (ref amount, value, () => Amount); }
		}

		private decimal cost;
		[Display (Name = "Цена")]
		[PropertyChangedAlso("Total")]
		public virtual decimal Cost {
			get => cost;
			set { SetField (ref cost, value, () => Cost); }
		}

		private string certificate;
		[Display(Name = "№ сертификата")]
		public virtual string Certificate {
			get => certificate;
			set { SetField(ref certificate, value, () => Certificate); }
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
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

		[Display(Name = "Собственник имущества")]
		public virtual Owner Owner {
			get => WarehouseOperation.Owner;
			set {
				if(WarehouseOperation.Owner != value) {
					WarehouseOperation.Owner = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion
		#region Расчетные
		public virtual string Title =>
			$"Поступление на склад {Nomenclature?.Name} в количестве {Amount} {Nomenclature?.Type?.Units?.Name}";

		public virtual decimal Total => Cost * Amount;
		public virtual StockPosition StockPosition => 
			new StockPosition(Nomenclature, WarehouseOperation.WearPercent, WearSize, Height, Owner);
		#endregion
		#region Не сохраняемые в базу свойства
		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get => WarehouseOperation.WearPercent;
			set => WarehouseOperation.WearPercent = value;
		}

		#endregion
		protected IncomeItem () { }
		public IncomeItem(Income income) {
			document = income;
		}

		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow) {
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
		}
		#endregion
	}
}

