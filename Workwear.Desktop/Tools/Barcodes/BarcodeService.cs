using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Tools.Barcodes 
{
	public class BarcodeService 
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public BarcodeService(BaseParameters baseParameters) {
			BaseCode = baseParameters.BarcodePrefix ?? 2000;
		}

		/// <summary>
		/// Стартовый код для серийных номеров. Коды начинающиеся с 2 зарезервированы под внутреннее использование на предприятии по стандарту EAN-13
		/// Мы в первые 3 цифры после 2-ки зашиваем код клиента, вернее последние 3 цифры кода клиента, для того чтобы штрих коды из разных баз отличались.
		/// Это параметр можно поменять в настройка базы BarcodePrefix
		/// </summary>
		public int BaseCode { get; } //2000-2999

		#region Create

		public IList<Barcode> CreateBarcodesInWarehouse(IUnitOfWork unitOfWork, Warehouse warehouse, StockPosition stockPosition, string label, int amount) 
		{
			if(unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));
			if(warehouse == null) throw new ArgumentNullException(nameof(warehouse));
			if(stockPosition == null) throw new ArgumentNullException(nameof(stockPosition));
			if(string.IsNullOrWhiteSpace(label)) throw new ArgumentNullException(nameof(label));
			if(amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

			IList<Barcode> barcodes = Create(unitOfWork, amount, stockPosition.Nomenclature, stockPosition.WearSize, stockPosition.Height);
			foreach(Barcode barcode in barcodes) 
			{
				barcode.Label = label;
				BarcodeOperation barcodeOperation = new BarcodeOperation()
				{
					Barcode = barcode,
					Warehouse = warehouse
				};
				unitOfWork.Save(barcodeOperation, false);
			}

			return barcodes;
		}
		
		public void CreateOrRemove(IUnitOfWork unitOfWork, IEnumerable<EmployeeIssueOperation> employeeIssueOperations) {
			foreach(var operation in employeeIssueOperations) {
				if(operation.Issued == operation.BarcodeOperations.Count)
					continue;

				if(operation.Issued > operation.BarcodeOperations.Count) {
					var barcodes = Create(unitOfWork, operation.Issued - operation.BarcodeOperations.Count, operation.Nomenclature, operation.WearSize, operation.Height);
					foreach(var barcode in barcodes) {
						var barcodeOperation = new BarcodeOperation {
							Barcode = barcode,
							EmployeeIssueOperation = operation
						};
						operation.BarcodeOperations.Add(barcodeOperation);
						unitOfWork.Save(barcodeOperation);
					}
				}
				else if(operation.Issued < operation.BarcodeOperations.Count) {
					var toRemove = operation.BarcodeOperations.OrderBy(x => x.Barcode.BarcodeOperations.Count).Take(operation.BarcodeOperations.Count - operation.Issued).ToArray();
					foreach(var removed in toRemove) {
						operation.BarcodeOperations.Remove(removed);
						if(removed.Id > 0)
							unitOfWork.Delete(removed);
						if(removed.Barcode.BarcodeOperations.All(x => x.Id == removed.Id)) //Пустая коллекция в этом услоыии тоже вернет true
							unitOfWork.Delete(removed.Barcode);
						else
							logger.Warn($"Штрихкод Id:{removed.Barcode.Id} не был удален, так как он уже используется в других операциях.");
					}
				}
			}
		}
		
		public IList<Barcode> Create(IUnitOfWork unitOfWork, int amount, Nomenclature nomenclature, Size size, Size height) {
			var barCodeList = new List<Barcode>();
			for(var i = 1; i < amount + 1; i++) {
				var newBarCode = new Barcode();
				newBarCode.Nomenclature = nomenclature;
				newBarCode.Size = size;
				newBarCode.Height = height;
				unitOfWork.Save(newBarCode);
				//Перезаписываем Title так как он формируется на основании полученного Id
				newBarCode.Title = $"{BaseCode}{newBarCode.Id:D8}{CheckSum($"{BaseCode}{newBarCode.Id:D8}")}";
				unitOfWork.Save(newBarCode);
				barCodeList.Add(newBarCode);
			}
			return barCodeList;
		}
		#endregion

		#region Barcodes Info

		public int GetAllBarcodesAmount(IUnitOfWork unitOfWork, int nomenclatureId) 
		{
			if(unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));
			if(nomenclatureId <= 0) throw new ArgumentOutOfRangeException(nameof(nomenclatureId));

			int barcodesInStock = unitOfWork.Session.QueryOver<BarcodeOperation>()
				.JoinQueryOver(bo => bo.Barcode)
				.Where(b => b.Nomenclature.Id == nomenclatureId)
				.SelectList(list => list
					.SelectCount(bo => bo.Barcode))
				.SingleOrDefault<int>();

			return barcodesInStock;
		}

		public int GetStockBarcodesAmount(IUnitOfWork unitOfWork, int nomenclatureId) 
		{
			if(unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));
			if(nomenclatureId <= 0) throw new ArgumentOutOfRangeException(nameof(nomenclatureId));
			
			BarcodeOperation barcodeOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
			Barcode barcodeAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;

			IQueryOver<BarcodeOperation, BarcodeOperation> barcodesInStock =
				unitOfWork.Session.QueryOver(() => barcodeOperationAlias)
					.JoinAlias(bo => bo.Barcode, () => barcodeAlias, JoinType.InnerJoin)
					.JoinAlias(bo => bo.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => barcodeAlias.Nomenclature, () => nomenclatureAlias, JoinType.InnerJoin)
					.JoinAlias(() => barcodeAlias.Size, () => sizeAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => barcodeAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
					.Where(() => nomenclatureAlias.Id == nomenclatureId)
					.Where(bo => bo.EmployeeIssueOperation == null)
					.Where(bo => bo.WarehouseOperation == null || warehouseOperationAlias.ReceiptWarehouse != null)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id)
						.SelectGroup(() => sizeAlias.Id)
						.SelectGroup(() => heightAlias.Id)
					);

			return barcodesInStock.RowCount();
		}
		#endregion
		
		#region Private Methods

		static int CheckSum(string upccode)
		{
			var sum = 0;
			var bOdd=false;
			foreach (var digit in upccode.Select(c => (int) Char.GetNumericValue(c)))
			{
				sum += bOdd ? digit * 3 : digit;
				bOdd = !bOdd;                       // switch every other character
			}
			var cs = 10 - sum % 10;
			return cs == 10? 0: cs;
		}
		
		#endregion
	}
}
