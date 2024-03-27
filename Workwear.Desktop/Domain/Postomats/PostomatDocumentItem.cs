using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Postomats {
	[Appellative(Gender = GrammaticalGender.Feminine, NominativePlural = "строки документа постамата", Nominative = "строка документа постамата")]
	[HistoryTrace]
	public class PostomatDocumentItem : PropertyChangedBase, IDomainObject{
		#region Cвойства
		public virtual int Id { get; set; }
		
		[IgnoreHistoryTrace]
		public virtual PostomatDocument Document { get; set; }
		
		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}
		
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
		
		private ServiceClaim serviceClaim;
		[Display(Name = "Заявка на обслуживание")]
		public virtual ServiceClaim ServiceClaim {
			get => serviceClaim;
			set => SetField(ref serviceClaim, value);
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
		
		#region Расчетные

		private CellLocation? location;
		public virtual CellLocation Location {
			get => location ?? new CellLocation("???", LocationStorage, LocationShelf, LocationCell);
			set {
				location = value;
				LocationStorage = location?.Storage ?? 0;
				LocationShelf = location?.Shelf ?? 0;
				LocationCell = location?.Cell ?? 0;
				OnPropertyChanged();
			}
		}

		public virtual uint CellNumber 
		{
			get => Location.Cell;
			set => _ = value;
		}
		
		public virtual string Title => Delta > 0 
			? $"Загрузка {Nomenclature.Name} х {Delta} в ячейку {Location.Title}"
		    : $"Выдача {Nomenclature.Name} х {-Delta} из ячейки {Location.Title}";
		#endregion
	}
	
	public struct CellLocation : IEquatable<CellLocation> {
		public readonly uint Storage;
		public readonly uint Shelf;
		public readonly uint Cell;
		public readonly string Number;

		public CellLocation(string number, uint storage, uint shelf, uint cell) {
			Number = number;
			Storage = storage;
			Shelf = shelf;
			Cell = cell;
		}
		
		public CellLocation(string number, QS.Cloud.Postomat.Manage.CellLocation location) {
			Number = number;
			Storage = location.Storage;
			Shelf = location.Shelf;
			Cell = location.Cell;
		}

		public bool IsEmpty => Storage == 0 && Shelf == 0 && Cell == 0;
		public string Title => IsEmpty ? null : $"{Number} ({Storage}-{Shelf}-{Cell})";
		
		public bool Equals(CellLocation other) {
			return Storage == other.Storage && Shelf == other.Shelf && Cell == other.Cell;
		}
	}
}
