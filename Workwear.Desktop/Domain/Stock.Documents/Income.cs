using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "приходные документы",
		Nominative = "приходный документ",
		Genitive = "приходного документа"
		)]
	[HistoryTrace]
	public class Income : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		#region Свойства

		//TODO При переводе диалога на VVMM перенести в VM
		public virtual bool SensitiveDocNumber => !AutoDocNumber;
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public virtual bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}
		public virtual string DocNumberText {
			get => AutoDocNumber ? (Id != 0 ? Id.ToString() : "авто" ) : DocNumber;
			set { 
				if(!AutoDocNumber) 
					DocNumber = value; 
			}
		}
		
		private Warehouse warehouse;
		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		private string number;
		[Display (Name = "Вх. номер")]
		public virtual string Number {
			get => number;
			set { SetField (ref number, value, () => Number); }
		}

		private IObservableList<IncomeItem> items = new ObservableList<IncomeItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<IncomeItem> Items {
			get => items;
			set { SetField (ref items, value, () => Items); }
		}
		#endregion
		public virtual string Title => $"Приходная накладная №{DocNumberText ?? Id.ToString()} от {Date:d}";
		
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});
			
			if (DocNumber != null && DocNumber.Length > 15)
				yield return new ValidationResult ("Номер документа должен быть не более 15 символов", 
					new[] { this.GetPropertyName (o => o.DocNumber)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if (Items.Any(i => i.Certificate != null && i.Certificate.Length > 40))
				yield return new ValidationResult("Длина номера сертификата не может быть больше 40 символов.",
					new[] { this.GetPropertyName(o => o.Items) });
		}

		#endregion
		public Income () { }

		#region Строки документа

		public virtual IncomeItem AddItem(Nomenclature nomenclature, IInteractiveMessage message) {
			if(nomenclature.Type == null) {
				//Такого в принципе быть не должно. Но бывают поломанные базы, поэтому лучше сообщить пользователю причину.
				message.ShowMessage(ImportanceLevel.Error, "У добавляемой номенклатуры обязательно должен быть указан тип.");
				return null;
			}
			
			var newItem = new IncomeItem (this) {
				Amount = 1,
				Nomenclature = nomenclature,
				Cost = nomenclature.SaleCost ?? 0m,
			};

			Items.Add (newItem);
			return newItem;
		}
		public virtual IncomeItem AddItem(
			Nomenclature nomenclature, 
			Size size, Size height, int amount = 0, 
			string certificate = null, decimal price = 0m, Owner owner = null)
		{
			var item = FindItem(nomenclature, size, height, owner);
			if(item == null) {
				item = new IncomeItem(this) {
					Amount = amount,
					Nomenclature = nomenclature,
					WearSize = size,
					Height = height,
					Cost = price,
					Certificate = certificate,
					Owner = owner
				};
				Items.Add(item);
			}
			else {
				item.Amount+= amount;
			}
			return item;
		}
		public virtual void RemoveItem(IncomeItem item) {
			Items.Remove (item);
		}

		public virtual IncomeItem FindItem(Nomenclature nomenclature, Size size, Size height, Owner owner) => Items
			.FirstOrDefault(i => i.Nomenclature.Id == nomenclature.Id
			                     && i.Height == height && i.WearSize == size && i.Owner == owner);
		#endregion

		public virtual void UpdateOperations(IUnitOfWork uow) {
			Items.ToList().ForEach(x => x.UpdateOperations(uow));
		}
	}
}

