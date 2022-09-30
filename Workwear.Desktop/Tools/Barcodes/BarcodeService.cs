using System;
using System.Linq;

namespace Workwear.Tools.Barcodes 
{
	public class BarcodeService 
	{
		public int BaseCode { get; } //2000-2999

		public BarcodeEan13 Create() {
			var freeCode = GetFreeProductCode();
			return new BarcodeEan13($"{BaseCode}{freeCode:D8}{CheckDigit(BaseCode, freeCode)}");
		}

		private int GetFreeProductCode() {
			return 1;
		}

		private int CheckDigit(int baseCode, int code) {
			var result = SumDigit(Int32.Parse(baseCode.ToString() + code));
			while(result > 9) {
				result = SumDigit(result);
			}
			return result;
		}

		private int SumDigit(int digit) => digit.ToString().Split().Sum(Int32.Parse);
	}
}
