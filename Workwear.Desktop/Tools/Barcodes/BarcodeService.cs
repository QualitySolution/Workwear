using System;
using System.Collections.Generic;
using System.Linq;
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
