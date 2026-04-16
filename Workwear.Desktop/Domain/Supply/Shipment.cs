using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Project.Domain;
using QS.Utilities;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Supply {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "планируемые поставки",
		Nominative = "планируемая поставка",
		Genitive = "планируемой поставки",
		GenitivePlural = "планируемых поставок"
	)]
	[HistoryTrace]
	public class Shipment: PropertyChangedBase, IDomainObject, IValidatableObject {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		#region Свойства

		public virtual int Id { get; set; }
		
		private DateTime? startPeriod;
		[Display(Name="Начало периода")]
		public virtual DateTime? StartPeriod {
			get => startPeriod;
			set { SetField(ref startPeriod, value); }
		}
		
		private DateTime? endPeriod;
		[Display(Name="Окончание периода")]
		public virtual DateTime? EndPeriod {
			get => endPeriod;
			set { SetField(ref endPeriod, value); }
		}
		
		private UserBase createdbyUser;
		[Display(Name = "Документ создал")]
		public virtual UserBase CreatedbyUser {
			get => createdbyUser; 
			set { SetField(ref createdbyUser, value); }
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set { SetField(ref comment, value); }
		}
		
		private DateTime creationDate = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate {
			get => creationDate;
			set { SetField(ref creationDate, value); }
		}

		private ShipmentStatus status;
		[Display(Name = "Статус поставки")]
		public virtual ShipmentStatus Status {
			get => status;
			set {
				if(SetField(ref status, value) && value == ShipmentStatus.Present && submitted == null)
					Submitted = DateTime.Now;
			}
		}	
		
		private DateTime? submitted;
		[Display(Name = "Передано в закупку")]
		public virtual DateTime? Submitted {
			get => submitted;
			set { SetField(ref submitted, value); }
		}
		
		private bool fullOrdered;
		[Display(Name = "Заказаны все запрошенные позиции")]
		public virtual bool FullOrdered {
			get => fullOrdered;
			set { SetField(ref fullOrdered, value); }
		}
		
		private bool fullReceived;
		[Display(Name = "Поставлены все заказанные позиции")]
		public virtual bool FullReceived {
			get => fullReceived;
			set { SetField(ref fullReceived, value); }
		}
		
		private bool hasReceive;
		[Display(Name = "Поставлены все заказанные позиции")]
		public virtual bool HasReceive {
			get => hasReceive;
			set { SetField(ref hasReceive, value); }
		}

		private DateTime? warehouseForecastingDate;
		[Display(Name = "Дата прогнозирования склада")]
		public virtual DateTime? WarehouseForecastingDate {
			get => warehouseForecastingDate;
			set => SetField(ref warehouseForecastingDate, value);
		}
		
		private IObservableList<ShipmentItem> items = new ObservableList<ShipmentItem>();
		[Display(Name = "Строки документа")]
		public virtual IObservableList<ShipmentItem> Items {
			get => items;
			set { SetField(ref items, value); }
		}
		
		private IObservableList<Income> incomes = new ObservableList<Income>();
		[Display(Name = "Связанные документы поступления")]
		public virtual IObservableList<Income> Incomes {
			get => incomes;
			set { SetField(ref incomes, value); }
		}
		#endregion

		public virtual string Title => $"Планируемая поставка № {Id.ToString()} в период с {StartPeriod:d} по {EndPeriod:d}";
		public virtual string PeriodTitle => DateHelper.GetDateRangeText(StartPeriod, EndPeriod);
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if ((Status == ShipmentStatus.Ordered || Status == ShipmentStatus.Received) && (StartPeriod == null || EndPeriod == null))
				yield return new ValidationResult ("Для статусов заказано и оприходовано должен быть указан период поставки.", 
					new[] { this.GetPropertyName (o => o.StartPeriod)});
			if (StartPeriod < new DateTime(2025, 1, 1))
				yield return new ValidationResult ("Период должен быть указан (не ранее 2025-го).", 
					new[] { this.GetPropertyName (o => o.StartPeriod)});
			if(StartPeriod > EndPeriod)
				yield return new ValidationResult("Начало периода поставки должно быть меньше его окончания.",
					new[] { this.GetPropertyName(o => o.StartPeriod) });
			if(Items.Count == 0)
				yield return new ValidationResult ("Поставка должна содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Requested <= 0))
				yield return new ValidationResult ("Поставка не должна содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any(i => i.StartPeriod > i.EndPeriod))
				yield return new ValidationResult("Начало периода в строках документа должно быть меньше его окончания.",
					new[] { this.GetPropertyName(o => o.StartPeriod) });


		}
		#endregion

		#region Строки документа

		public virtual ShipmentItem AddItem(Nomenclature nomenclature, IInteractiveMessage message) {
			if(nomenclature == null)
				throw new ArgumentNullException(nameof(nomenclature));

			var newItem = new ShipmentItem(this) {
				Requested = 1,
				Nomenclature = nomenclature,
				Cost = nomenclature.SaleCost ?? 0m,
			};
			Items.Add(newItem);
			return newItem;
		}

		public virtual ShipmentItem AddItem(Nomenclature nomenclature, Size size, Size height, int amount = 0, decimal price = 0m) {
			if(nomenclature == null)
				throw new ArgumentNullException(nameof(nomenclature));
			
			var item = FindItem(nomenclature,size,height);
			if(item == null) {
				item = new ShipmentItem(this) {
					Requested = amount,
					Nomenclature = nomenclature,
					WearSize = size,
					Height = height,
					Cost = price
				};
				Items.Add(item);
			}
			else {
				item.Requested += amount;
			}
			return item;
		}

		public virtual void RemoveItem(ShipmentItem item) {
			Items.Remove(item);
		}

		public virtual ShipmentItem FindItem(Nomenclature nomenclature, Size size, Size height) =>Items.
			FirstOrDefault(i=>i.Nomenclature.Id == nomenclature.Id && i.Height==height&&i.WearSize==size);

		#endregion
	}
}
