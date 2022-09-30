using System;

namespace Workwear.Tools.Barcodes 
{
	public class BarcodeEan13 
	{
		public BarcodeEan13(string value) {
			if(value.Length > 13)
				throw new ArgumentOutOfRangeException();
			Value = value;
		}
		public string Value { get; }
	}
}
