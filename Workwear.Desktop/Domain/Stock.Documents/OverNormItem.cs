using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents 
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи вне нормы",
		Nominative = "строка выдачи вне нормы",
		Genitive = "строки выдачи вне норм"
	)]
	[HistoryTrace]
	public class OverNormItem : PropertyChangedBase, IDomainObject
	{
		#region Properties
		public virtual int Id { get; set; }

		private OverNorm document;
		[Display(Name = "Документ выдачи вне нормы")]
		public virtual OverNorm Document {
			get => document;
			set => SetField(ref document, value);
		}
		
		private OverNormOperation overNormOperation;
		[Display(Name = "Операция")]
		public virtual OverNormOperation OverNormOperation {
			get => overNormOperation;
			set {
				if(overNormOperation == value)
					return;

				UnsubscribeFromBarcodeOperations();
				overNormOperation = value;
				SubscribeToBarcodeOperations();
				OnPropertyChanged();
				OnPropertyChanged(nameof(Barcodes));
				OnPropertyChanged(nameof(CanEditAmount));
			}
		}

		#endregion
		
		#region Not Mapped Propertis
		public virtual EmployeeCard Employee => OverNormOperation.Employee;

		public virtual IEnumerable<Barcode> Barcodes => OverNormOperation.BarcodeOperations?.Select(b => b.Barcode);

		public virtual int Amount {
			get => OverNormOperation?.WarehouseOperation?.Amount ?? 0;
			set {
				if(OverNormOperation?.WarehouseOperation == null)
					return;
				var amount = Math.Max(1, Math.Min(value, MaxAmount));
				if(OverNormOperation.WarehouseOperation.Amount == amount)
					return;
				OverNormOperation.WarehouseOperation.Amount = amount;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Title));
			}
		}

		public virtual int MaxAmount { get; set; } = int.MaxValue;

		public virtual bool CanEditAmount =>
			OverNormOperation?.WarehouseOperation != null
			&& OverNormOperation.BarcodeOperations?.Any() != true;
			
		public virtual string Title => $"Строка выдачи вне нормы ({OverNormOperation.Type.GetAttribute<DisplayAttribute>().Name}) {OverNormOperation.WarehouseOperation.Nomenclature.Name} в количестве {OverNormOperation.WarehouseOperation.Amount}";
		#endregion
		
		protected OverNormItem() 
		{
		}
		
		public OverNormItem(OverNorm document, OverNormOperation operation) 
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			OverNormOperation = operation ?? throw new ArgumentNullException(nameof(operation));
		}

		private void SubscribeToBarcodeOperations()
		{
			if(OverNormOperation?.BarcodeOperations is INotifyCollectionChanged barcodeOperations)
				barcodeOperations.CollectionChanged += BarcodeOperations_CollectionChanged;
		}

		private void UnsubscribeFromBarcodeOperations()
		{
			if(OverNormOperation?.BarcodeOperations is INotifyCollectionChanged barcodeOperations)
				barcodeOperations.CollectionChanged -= BarcodeOperations_CollectionChanged;
		}

		private void BarcodeOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Barcodes));
			OnPropertyChanged(nameof(CanEditAmount));
		}
	}
}
