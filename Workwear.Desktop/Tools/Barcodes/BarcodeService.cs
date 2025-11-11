using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Tools.Barcodes 
{
	public class BarcodeService 
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public BarcodeService(BaseParameters baseParameters) {
			BaseCode = baseParameters.BarcodePrefix ?? 2001;
		}

		/// <summary>
		/// Стартовый код для серийных номеров. Коды начинающиеся с 2 зарезервированы под внутреннее использование на предприятии по стандарту EAN-13
		/// Мы в первые 3 цифры после 2-ки зашиваем код клиента, вернее последние 3 цифры кода клиента, для того чтобы штрих коды из разных баз отличались.
		/// Это параметр можно поменять в настройка базы BarcodePrefix
		/// </summary>
		public int BaseCode { get; } //2001-2999

		#region Create

		public void CreateOrRemoveEAN13(IUnitOfWork unitOfWork, IEnumerable<EmployeeIssueOperation> employeeIssueOperations) {
			foreach(var operation in employeeIssueOperations) {
				int bcount = operation.BarcodeOperations.Count;
				if(operation.Issued == bcount)
					continue;

				if(operation.Issued > bcount) {
					var barcodes = Create(unitOfWork, operation.Issued - bcount, operation.Nomenclature, operation.WearSize, operation.Height);
					foreach(var barcode in barcodes) {
						var barcodeOperation = new BarcodeOperation {
							Barcode = barcode,
							EmployeeIssueOperation = operation
						};
						operation.BarcodeOperations.Add(barcodeOperation);
						unitOfWork.Save(barcodeOperation);
					}
				}
				else if(operation.Issued < bcount) {
					var toRemove = operation.BarcodeOperations.Where(b => b.Barcode.Type == BarcodeTypes.EAN13)
						.OrderBy(x => x.Barcode.BarcodeOperations.Count).Take(bcount - operation.Issued).ToArray();
					foreach(var removed in toRemove) {
						operation.BarcodeOperations.Remove(removed);
						if(removed.Id > 0)
							unitOfWork.Delete(removed);
						if(removed.Barcode.BarcodeOperations.All(x => x.Id == removed.Id)) //Пустая коллекция в этом услоыии тоже вернет true
							unitOfWork.Delete(removed.Barcode);
						else
							logger.Warn($"Метка(штрихкод) Id:{removed.Barcode.Id} не была удалена, так как она уже используется в других операциях.");
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
				newBarCode.Title = "generated" + new Random().Next(); //т.к. в базе not null, а далее уже нужен id
				unitOfWork.Save(newBarCode);
				//Перезаписываем Title так как он формируется на основании полученного Id
				newBarCode.Title = $"{BaseCode}{newBarCode.Id:D8}{CheckSum($"{BaseCode}{newBarCode.Id:D8}")}";
				unitOfWork.Save(newBarCode);
				barCodeList.Add(newBarCode);
			}
			return barCodeList;
		}

		public static void SetClothingServiceCode(IUnitOfWork uow, Service service) {
			uow.Save(service);
			service.Code = $"2000{service.Id:D8}{CheckSum($"2000{service.Id:D8}")}";
			uow.Save(service);
		}

		public static bool CheckBarcode(string barcode, BarcodeTypes type) {
			switch(type) {
				case BarcodeTypes.EAN13:
					return barcode.Length == 13 && barcode.All(char.IsDigit) && CheckSum(barcode.Substring(0, 12)).ToString()[0] == barcode[12];
				case BarcodeTypes.EPC96:
					return barcode.Length == 24 && System.Text.RegularExpressions.Regex.IsMatch(barcode, @"^[0-9A-Fa-f]+$");
				default:
					throw new NotImplementedException(typeof(BarcodeTypes)+ " in " + type);
			}
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
