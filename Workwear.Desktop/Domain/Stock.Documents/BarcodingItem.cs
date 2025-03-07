using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents {
		[Appellative(Gender = GrammaticalGender.Feminine,
			NominativePlural = "строки маркировки",
			Nominative = "строка маркировки",
			Genitive = "строки маркировки",
			PrepositionalPlural = "строках маркировки"
		)]
		
	public class BarcodingItem : PropertyChangedBase, IDomainObject {
		
		#region Свойства

		public int Id { get; }

		public virtual string Title => "";
			//$"{Employee.ShortName} - переоценка {Nomenclature?.Name} в количестве {Amount} {Nomenclature?.Type?.Units?.Name}";
		
		private Barcoding document;
		[Display(Name = "Документ маркировки")]
		[IgnoreHistoryTrace]
		public virtual Barcoding Document {
			get => document;
			set => SetField(ref document, value);
		}

		private WarehouseOperation operationReceipt = new WarehouseOperation();
		[Display(Name = "Операция поступления с маркировкой")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation OperationReceipt {
			get => operationReceipt;
			set => SetField(ref operationReceipt, value);
		}
		
		private WarehouseOperation operationExpence = new WarehouseOperation();
		[Display(Name = "Операция списания со склада")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation OperationExpence {
			get => operationExpence;
			set => SetField(ref operationExpence, value);
		}
		#endregion
	}
}
