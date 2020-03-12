using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "строки перевода со склада на склад",
	Nominative = "строка перевода со склада на склад")]
	public class TransferItem : PropertyChangedBase, IDomainObject
	{

		#region Свойства

		public virtual int Id { get; set; }

		private Transfer document;

		[Display(Name = "Документ")]
		public virtual Transfer Document {
			get { return document; }
			set { SetField(ref document, value); }
		}


		Nomenclature nomenclature;

		[Display(Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		int amount;

		[Display(Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get { return amount; }
			set { SetField(ref amount, value, () => Amount); }
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation {
			get { return warehouseOperation; }
			set { SetField(ref warehouseOperation, value); }
		}

		string size;

		[Display(Name = "Размер")]
		public virtual string Size {
			get { return size; }
			set { SetField(ref size, value, () => Size); }
		}

		string wearGrowth;

		[Display(Name = "Рост одежды")]
		public virtual string WearGrowth {
			get { return wearGrowth; }
			set { SetField(ref wearGrowth, value, () => WearGrowth); }
		}


		#region Расчетные

		public virtual string Title {
			get {
				return String.Format("Перевод со склада {0} на склад {1}",
			  document.WarehouseTo,
			  document.WarehouseFrom
		  );
			}
		}

		#endregion

		#endregion

		public TransferItem()
		{

		}

		public TransferItem(Transfer transfer)
		{
			this.document = transfer;
		}

		#region Функции

		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
		}

		#endregion
	}
}
