﻿using System;
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
		
		/// <summary>
		/// Сгенерировать набор штрихкодов
		/// </summary>
		/// <param name="unitOfWork">В котором будут созданы и сохранены, без комита</param>
		/// <param name="label">до 100 символов</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public IList<Barcode> Create(IUnitOfWork unitOfWork, int amount, Nomenclature nomenclature, Size size, Size height, string label = "") {
			if (label?.Length > 100)
				throw new ArgumentOutOfRangeException(nameof(label));
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
		public int CountAllBarcodes(IUnitOfWork unitOfWork, StockPosition stockPosition)
//public int CountAllBarcodes(IUnitOfWork unitOfWork, Nomenclature nomenclature, Size size = null, Size height = null) 
		{
			if (unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));
			if (stockPosition == null) throw new ArgumentNullException(nameof(stockPosition));

			Barcode bAlias = null;
			int barcodesInStock = unitOfWork.Session.QueryOver<BarcodeOperation>()
				.JoinAlias(bo => bo.Barcode, () => bAlias)
				.Where(b => bAlias.Nomenclature == stockPosition.Nomenclature && bAlias.Size == stockPosition.WearSize && bAlias.Height == stockPosition.Height)
				.SelectList(list => list
					.SelectCountDistinct(() => bAlias.Id)
				)
				.SingleOrDefault<int>();

			return barcodesInStock;
		}
		#endregion
		
		#region Barcodes In Stock
		public int CountBalanceInStock(IUnitOfWork uow, Nomenclature nomenclature, Size size = null, Size height = null, Warehouse warehouse = null) 
		{
			if (nomenclature == null) throw new ArgumentNullException(nameof(nomenclature));
			return BalanceFreeBarcodesQuery(uow, nomenclature, size, height).RowCount();
		}

		public IList<Barcode> GetFreeBarcodes(IUnitOfWork uow, Nomenclature nomenclature, Size size = null, Size height = null, Warehouse warehouse = null) 
		{
			if (uow == null) throw new ArgumentNullException(nameof(uow));
			if (nomenclature == null) throw new ArgumentNullException(nameof(nomenclature));

			return BalanceFreeBarcodesQuery(uow, nomenclature, size, height).Select(x => x.Barcode).List<Barcode>();
		}
////1298 Возможно удалить
		public IList<int> GetFreeBarcodesIds(IUnitOfWork uow, Nomenclature nomenclature, Size size = null, Size height = null, Warehouse warehouse = null) 
		{
			if (uow == null) throw new ArgumentNullException(nameof(uow));
			if (nomenclature == null) throw new ArgumentNullException(nameof(nomenclature));
			
			return BalanceFreeBarcodesQuery(uow, nomenclature, size, height).Select(x => x.Barcode.Id).List<int>();
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
		
		private IQueryOver<BarcodeOperation, BarcodeOperation> BalanceFreeBarcodesQuery(IUnitOfWork uow, Nomenclature nomenclature, Size size = null, Size height = null) 
		{
			Barcode bSubAlias = null;
			BarcodeOperation boSubAlias = null;
			
			Barcode bSub1Alias = null;
			BarcodeOperation boSubAlias1 = null;
			OverNormOperation oonSubAlias1 = null;
			WarehouseOperation woSubAlias1 = null;
			
			BarcodeOperation boAlias = null;
			OverNormOperation oonAlias = null;
			WarehouseOperation woAlias = null;

			var subQuery = QueryOver.Of(() => boSubAlias)
				.JoinAlias(() => boSubAlias.Barcode, () => bSubAlias, JoinType.InnerJoin)
				.Where(() => bSubAlias.Nomenclature == nomenclature && bSubAlias.Size == size && bSubAlias.Height == height)
				.Select(Projections.Group(() => bSubAlias.Id))
				.Where(Restrictions.Eq(Projections.Count(() => bSubAlias.Id), 1))
				.Where(x => x.Barcode == boAlias.Barcode);
				
			var subQuery1 = QueryOver.Of(() => boSubAlias1)
				.JoinAlias(() => boSubAlias1.Barcode, () => bSub1Alias)
				.JoinAlias(() => boSubAlias1.OverNormOperation, () => oonSubAlias1)
				.JoinAlias(() => oonSubAlias1.WarehouseOperation, () => woSubAlias1)
				.Where(() => boSubAlias1.OverNormOperation != null &&
				             woSubAlias1.Nomenclature == nomenclature &&
				             woSubAlias1.WearSize == size &&
				             woSubAlias1.Height == height)
				.SelectList(list => list
					.SelectGroup(() => bSub1Alias.Id)
					.SelectMax(() => oonSubAlias1.OperationTime))
				.Where(x => x.Barcode == boAlias.Barcode && oonAlias.OperationTime > oonSubAlias1.OperationTime);

			return uow.Session.QueryOver(() => boAlias)
				.JoinAlias(() => boAlias.OverNormOperation, () => oonAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => oonAlias.WarehouseOperation, () => woAlias, JoinType.LeftOuterJoin)
				.Where(Restrictions.Disjunction()
					.Add(Subqueries.WhereExists(subQuery))
					.Add(Subqueries.WhereExists(subQuery1)));
		}

		#endregion
	}
}
