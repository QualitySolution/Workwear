using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Stock.Barcodes;

namespace Workwear.Tools.Barcodes 
{
	public class BarcodeService 
	{
		public int BaseCode { get; } //2000-2999

		public IList<BarcodeEan13> Create(IUnitOfWork unitOfWork, int amount) {
			var barCodeList = new List<BarcodeEan13>();
			for(var i = 1; i < amount + 1; i++) {
				var newBarCode = new BarcodeEan13();
				unitOfWork.Save(newBarCode);
				newBarCode.Value = $"{BaseCode}{newBarCode.Id:D8}{GetCheckDigit(BaseCode, newBarCode.Id)}";
				newBarCode.Fractional = $"{i}/{amount}";
				barCodeList.Add(newBarCode);
			}
			return barCodeList;
		}

		private int GetCheckDigit(int baseCode, int code) {
			var result = SumDigit(Int32.Parse(baseCode.ToString() + code));
			while(result > 9) {
				result = SumDigit(result);
			}
			return result;
		}

		private int SumDigit(int digit) => digit.ToString().Split().Sum(Int32.Parse);
	}
}
