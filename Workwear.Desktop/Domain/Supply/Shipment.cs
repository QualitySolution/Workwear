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
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Supply {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "предполагаемые поставки",
		Nominative = "предполагаемая поставка",
		Genitive = "предполагаемой поставки"
	)]
	[HistoryTrace]
	public class Shipment: IDomainObject {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		#region Свойства

		public virtual int Id { get; set; }
		
		private DateTime startPeriod;
		[Display(Name="Начало периода")]
		public virtual DateTime StartPeriod {
			get=> startPeriod;
			set {startPeriod = value;}
		}
		
		private DateTime endPeriod;
		[Display(Name="Окончание периода")]
		public virtual DateTime EndPeriod {
			get=> endPeriod;
			set { endPeriod = value; }
		}
		
		private UserBase createdbyUser;
		[Display(Name = "Документ создал")]
		public virtual UserBase CreatedbyUser {
			get =>createdbyUser; 
			set { createdbyUser = value; }
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get =>comment;
			set { comment = value; }
		}
		
		private DateTime creationDate = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate {
			get => creationDate;
			set {creationDate = value;}
		}
		
		private IObservableList<ShipmentItem> items = new ObservableList<ShipmentItem>();

		[Display(Name = "Строки документа")]
		public virtual IObservableList<ShipmentItem> Items {
			get=>items;
			set { items = value; }
		}

		#endregion

		public virtual string Title => $"Предполагаемая поставка № {Id.ToString()} в период с {StartPeriod:d} по {EndPeriod:d}";
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if (StartPeriod < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата начала периода должна быть указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.StartPeriod)});
			if(StartPeriod > EndPeriod)
				yield return new ValidationResult("Дата начала периода должна быть меньше его окончания",
					new[] { this.GetPropertyName(o => o.StartPeriod) });
			
		}
		#endregion

		#region Строки документа

		public virtual ShipmentItem AddItem(Nomenclature nomenclature, IInteractiveMessage message) {
			var newItem = new ShipmentItem(this) {
				Amount = 1,
				Nomenclature = nomenclature,
				Cost = nomenclature.SaleCost ?? 0m,
			};
			Items.Add(newItem);
			return newItem;
		}

		public virtual ShipmentItem AddItem(Nomenclature nomenclature, Size size, Size height, int amount = 0, decimal price = 0m) {
			var item = FindItem(nomenclature,size,height);
			if(item == null) {
				item = new ShipmentItem(this) {
					Amount = amount,
					Nomenclature = nomenclature,
					WearSize = size,
					Height = height,
					Cost = price
				};
				Items.Add(item);
			}
			else {
				item.Amount += amount;
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
