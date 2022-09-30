namespace Workwear.Tools.Barcodes 
{
	public class BarcodeService 
	{
		public string ClientCode { get; } //2
		public string BaseCode { get; } //3
		
		public BarcodeEan13 Create() 
		{
			return new BarcodeEan13($"{ClientCode}{BaseCode}{GetFreeProductCode()}");
		}

		public string GetFreeProductCode() {
			return "00000001";
		}
	}
}
