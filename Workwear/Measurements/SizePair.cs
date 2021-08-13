namespace workwear.Measurements
{
	public class SizePair
	{
		public string StandardCode { get; private set; }
		public string Size { get; private set; }

		public SizePair(string std, string size)
		{
			StandardCode = std;
			Size = size;
		}
	}
}

